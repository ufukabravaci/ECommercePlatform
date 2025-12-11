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
    bool RequiresEmailConfirmation,
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
    IUserRefreshTokenRepository refreshTokenRepository,
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
            // ... (Lockout kodu aynı) ...
            var endPoint = await userManager.GetLockoutEndDateAsync(user);
            var remaining = endPoint.Value - DateTimeOffset.Now;
            return Result<LoginResponse>.Failure($"Hesabınız kilitlendi. Lütfen {Math.Ceiling(remaining.TotalMinutes)} dakika sonra tekrar deneyin.");
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
        // 3. Reset Access Failed Count
        await userManager.ResetAccessFailedCountAsync(user);

        // 4. Check Email Confirmation
        if (!user.EmailConfirmed) // DB'ye gitmeye gerek yok, user nesnesinde zaten var
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

            string body = $@"
                <h3>Email Onayı Gerekli</h3>
                <p>Giriş yapabilmek için lütfen email adresinizi doğrulayın. Onay kodunuz:</p>
                <h2 style='letter-spacing: 5px; background-color: #eee; display: inline-block; padding: 5px;'>{code}</h2>
                <p>3 dakika geçerlidir.</p>";

            await mailService.SendAsync(user.Email!, "Email Doğrulama", body, cancellationToken);

            return new LoginResponse(null, null, false, true, "Email onayı gerekli. Doğrulama kodu gönderildi.");
        }

        // 5. Check 2FA
        if (user.TwoFactorEnabled)
        {
            var code = await userManager.GenerateTwoFactorTokenAsync(user, "SixDigit");

            string body = $@"
                <h3>Giriş Doğrulama</h3>
                <p>Giriş kodunuz:</p>
                <h2 style='letter-spacing: 5px; background-color: #eee; display: inline-block; padding: 5px;'>{code}</h2>
                <p>3 dakika geçerlidir.</p>";

            await mailService.SendAsync(user.Email!, "2FA Doğrulama Kodu", body, cancellationToken);

            return new LoginResponse(null, null, true, false, "Doğrulama kodu email adresinize gönderildi.");
        }

        // 6. Generate Tokens (Normal Login)
        string accessToken = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        string refreshToken = jwtProvider.CreateRefreshToken();

        var refreshTokenEntity = new UserRefreshToken
        {
            Code = refreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            UserId = user.Id // Foreign Key ile bağlıyoruz
        };

        refreshTokenRepository.Add(refreshTokenEntity);

        // Bu SaveChanges sadece UserRefreshToken tablosuna insert atar.
        // User tablosuna update atmaz, dolayısıyla Concurrency hatası olmaz.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, refreshToken, false, false, "Giriş başarılı.");
    }
}
