using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.RefreshToken;

public sealed record RevokeAllCommand : IRequest<Result<string>>;

public sealed class RevokeAllCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeAllCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RevokeAllCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.GetUserId();
        if (userId == Guid.Empty) return Result<string>.Failure("Kullanıcı bulunamadı.");

        var user = await userRepository.GetAll()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı.");

        // Henüz revoke edilmemiş ve süresi dolmamış tüm tokenları iptal et
        foreach (var token in user.RefreshTokens.Where(r => r.IsValid))
        {
            token.RevokedAt = DateTimeOffset.Now;
            token.RevokedByIp = "GlobalLogout";
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Tüm cihazlardan çıkış yapıldı.";
    }
}
