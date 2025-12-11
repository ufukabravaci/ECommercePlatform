using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword) : IRequest<Result<string>>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(p => p.Email).EmailAddress().NotEmpty();
        RuleFor(p => p.Password).NotEmpty().MinimumLength(6);
        RuleFor(p => p.ConfirmPassword).Equal(p => p.Password).WithMessage("Şifreler eşleşmiyor.");
        RuleFor(p => p.UserName).NotEmpty().MinimumLength(3);
    }
}

public sealed class RegisterCommandHandler(
    UserManager<User> userManager,
    IEmailService mailService) : IRequestHandler<RegisterCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.FirstName, request.LastName, request.Email, request.UserName);

        // 2FA varsayılan olarak kapalı gelsin, kullanıcı profilden açsın.

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
        }

        // Email Onay Kodu Gönderimi
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Token'ı linkleyebilirsin veya kod olarak gönderebilirsin.
        string subject = "E-Ticaret Platformu - Email Onayı";
        string body = $"<h3>Hoşgeldiniz!</h3><p>Kaydınızı tamamlamak için onay kodunuz: <h2 style='color:blue;'>{token}</h2></p><p>Bu kod 3 dakika geçerlidir.</p>";
        await mailService.SendAsync(user.Email!, subject, body, cancellationToken);

        return "Kayıt başarılı. Lütfen email adresinize gönderilen kod ile hesabınızı doğrulayın.";
    }
}