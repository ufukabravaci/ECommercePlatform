using ECommercePlatform.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Register;

public sealed record ConfirmEmailCommand(string Email, string Token) : IRequest<Result<string>>;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(p => p.Email).EmailAddress().NotEmpty();
        RuleFor(p => p.Token).NotEmpty();
    }
}

public sealed class ConfirmEmailCommandHandler(UserManager<User> userManager)
    : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı.");

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            return Result<string>.Failure("Email doğrulanamadı. Token geçersiz veya süresi dolmuş.");

        return "Email başarıyla doğrulandı. Giriş yapabilirsiniz.";
    }
}