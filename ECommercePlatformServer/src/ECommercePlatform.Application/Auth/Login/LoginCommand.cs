using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Login;

public sealed record LoginCommand(string EmailOrUserName, string Password) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    string? Message);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    UserManager<User> userManager,
    IJwtProvider jwtProvider,
    IEmailService mailService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user;
        if (request.EmailOrUserName.Contains("@"))
            user = await userManager.FindByEmailAsync(request.EmailOrUserName);
        else
            user = await userManager.FindByNameAsync(request.EmailOrUserName);

        if (user is null) return Result<LoginResponse>.Failure("Kullanıcı adı veya şifre hatalı.");

        // 1. Check Lockout
        if (await userManager.IsLockedOutAsync(user))
        {
            var endPoint = await userManager.GetLockoutEndDateAsync(user);
            var remaining = endPoint.Value - DateTimeOffset.Now;
            return Result<LoginResponse>.Failure($"Hesabınız çok fazla başarısız giriş denemesi nedeniyle kilitlendi. Lütfen {Math.Ceiling(remaining.TotalMinutes)} dakika sonra tekrar deneyin.");
        }

        // 2. Check Password
        var checkPassword = await userManager.CheckPasswordAsync(user, request.Password);

        if (!checkPassword)
        {
            // Başarısız sayacını 1 artır
            await userManager.AccessFailedAsync(user);

            // Eğer limit dolduysa Identity otomatik olarak LockoutEnd tarihini set eder.
            if (await userManager.IsLockedOutAsync(user))
            {
                return Result<LoginResponse>.Failure("Hesabınız 5 başarısız deneme nedeniyle 15 dakika kilitlendi.");
            }

            return Result<LoginResponse>.Failure("Kullanıcı adı veya şifre hatalı.");
        }
        // Başarılı girişte başarısız sayacını sıfırla
        await userManager.ResetAccessFailedCountAsync(user);

        // 3. Check Email Confirmation
        if (!await userManager.IsEmailConfirmedAsync(user))
            return Result<LoginResponse>.Failure("Lütfen önce email adresinizi doğrulayın.");

        // 4. Check 2FA
        if (user.TwoFactorEnabled)
        {
            // Identity'nin Email provider'ı üzerinden kod üret
            var code = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            await mailService.SendAsync(
                user.Email!,
                "2FA Doğrulama Kodu",
                $"Giriş kodunuz: <strong>{code}</strong>",
                cancellationToken);

            return new LoginResponse(null, null, true, "Doğrulama kodu email adresinize gönderildi.");
        }

        // 5. Generate Tokens (Normal Login)
        string accessToken = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        string refreshToken = jwtProvider.CreateRefreshToken();

        user.RefreshTokens.Add(new UserRefreshToken
        {
            Code = refreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            UserId = user.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, refreshToken, false, "Giriş başarılı.");
    }
}
