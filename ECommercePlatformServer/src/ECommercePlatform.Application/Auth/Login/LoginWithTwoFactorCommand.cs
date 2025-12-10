using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Login;

public sealed record LoginWithTwoFactorCommand(string Email, string Code) : IRequest<Result<LoginWithTwoFactorResponse>>;

public sealed record LoginWithTwoFactorResponse(string AccessToken, string RefreshToken);

public sealed class LoginWithTwoFactorCommandValidator : AbstractValidator<LoginWithTwoFactorCommand>
{
    public LoginWithTwoFactorCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
    }
}

public sealed class LoginWithTwoFactorCommandHandler(
    UserManager<User> userManager,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginWithTwoFactorCommand, Result<LoginWithTwoFactorResponse>>
{
    public async Task<Result<LoginWithTwoFactorResponse>> Handle(LoginWithTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result<LoginWithTwoFactorResponse>.Failure("Kullanıcı bulunamadı.");

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Code);
        if (!isValid)
            return Result<LoginWithTwoFactorResponse>.Failure("Geçersiz doğrulama kodu.");

        // Token üret
        string accessToken = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        string refreshToken = jwtProvider.CreateRefreshToken();

        user.RefreshTokens.Add(new UserRefreshToken
        {
            Code = refreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            UserId = user.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginWithTwoFactorResponse(accessToken, refreshToken);
    }
}
