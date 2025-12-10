using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;
public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken);

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository) // RefreshTokenlar user üzerinde
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Token'a sahip kullanıcıyı bul (User.RefreshTokens koleksiyonundan)
        // IUserRepository üzerinden Include yaparak çekmemiz lazım
        var user = await userRepository.GetAll()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Code == request.RefreshToken), cancellationToken);

        if (user is null) return Result<RefreshTokenResponse>.Failure("Token geçersiz.");

        var existingToken = user.RefreshTokens.Single(t => t.Code == request.RefreshToken);

        // Validasyonlar
        if (existingToken.RevokedAt is not null)
        {
            // Token çalınmış olabilir. Ekstra güvenlik önlemleri alınabilir.
            // Fakat bu örnekte sadece hata döndürüyoruz.
            return Result<RefreshTokenResponse>.Failure("Bu token daha önce kullanılmış (Revoked).");
        }

        if (existingToken.IsExpired)
        {
            return Result<RefreshTokenResponse>.Failure("Oturum süresi dolmuş. Tekrar giriş yapın.");
        }

        // Token Rotation
        string newRefreshToken = jwtProvider.CreateRefreshToken();
        string newAccessToken = await jwtProvider.CreateTokenAsync(user, cancellationToken);

        // Eskiyi revoke et
        existingToken.RevokedAt = DateTimeOffset.Now;
        existingToken.ReplacedByToken = newRefreshToken;
        // Ip bilgisi HttpContextAccessor'dan alınabilir ama şimdilik gerek yok.

        // Yeniyi ekle
        user.RefreshTokens.Add(new UserRefreshToken
        {
            Code = newRefreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            UserId = user.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}
