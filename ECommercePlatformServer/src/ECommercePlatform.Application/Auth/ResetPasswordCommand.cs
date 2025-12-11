using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Result<string>>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(p => p.Email).EmailAddress().NotEmpty();
        RuleFor(p => p.Token).NotEmpty();
        RuleFor(p => p.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(p => p.ConfirmNewPassword).Equal(p => p.NewPassword).WithMessage("Şifreler uyuşmuyor.");
    }
}

public sealed class ResetPasswordCommandHandler(UserManager<User> userManager,
    IUserRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı.");

        // Identity kütüphanesi token'ı doğrular ve şifreyi değiştirir.
        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
        }

        var activeTokens = await refreshTokenRepository
            .WhereWithTracking(t => t.UserId == user.Id && t.RevokedAt == null &&
            t.Expiration > DateTimeOffset.Now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.Now;
            token.RevokedByIp = "PasswordReset";
            token.ReplacedByToken = null;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
    }
}
