using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email).EmailAddress().NotEmpty();
    }
}

public sealed class ForgotPasswordCommandHandler(
    UserManager<User> userManager,
    IEmailService mailService) : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // RateLimiter ile istek sayısı kısıtlı tutulacak.
            // Ancak yine de sistemde bu mailin kayıtlı olup olmadığına dair bilgi verilmemli.
            return "Eğer sistemde kayıtlıysa, şifre sıfırlama linki gönderildi.";
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        string subject = "Şifre Sıfırlama Kodu";
        string body = $@"
            <h3>Şifre Sıfırlama</h3>
            <p>Hesabınızın şifresini sıfırlamak için onay kodunuz:</p>
            <h2 style='background-color: #eee; display: inline-block; padding: 10px; letter-spacing: 5px;'>{token}</h2>
            <p>Bu kod 3 dakika geçerlidir.</p>";

        await mailService.SendAsync(user.Email!, subject, body, cancellationToken);

        return "Şifre sıfırlama maili gönderildi.";
    }
}
