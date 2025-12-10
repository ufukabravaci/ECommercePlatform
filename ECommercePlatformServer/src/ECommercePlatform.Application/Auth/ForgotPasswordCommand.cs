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

        // Link oluşturma kısmı genelde Client URL'i ile birleştirilir.
        // Basitlik için sadece token gönderiyorum.
        string body = $"Şifrenizi sıfırlamak için kodunuz: <strong>{token}</strong>";

        await mailService.SendAsync(user.Email!, "Şifre Sıfırlama Talebi", body, cancellationToken);

        return "Şifre sıfırlama maili gönderildi.";
    }
}
