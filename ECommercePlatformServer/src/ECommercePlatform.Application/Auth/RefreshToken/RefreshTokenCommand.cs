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
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("RefreshToken alanı boş olamaz.");
    }
}

public sealed class RefreshTokenCommandHandler(
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Token'a sahip kullanıcıyı ve CompanyUser bağını getir
        var user = await userRepository.GetAll()
            .Include(u => u.RefreshTokens)
                .ThenInclude(t => t.CompanyUser) // Tenant bilgisi şart
            .FirstOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Code == request.RefreshToken),
                cancellationToken);

        if (user is null)
            return Result<RefreshTokenResponse>.Failure("Token geçersiz.");

        var existingToken = user.RefreshTokens
            .Single(t => t.Code == request.RefreshToken);

        // 2. Validasyonlar
        if (!existingToken.IsActive)
            return Result<RefreshTokenResponse>.Failure("Token geçersiz veya süresi dolmuş.");

        // --- DÜZELTME: Sadece CompanyUser kontrolü yapıyoruz ---
        if (existingToken.CompanyUser is null)
        {
            return Result<RefreshTokenResponse>.Failure("Oturum bilgisi (Tenant) bulunamadı. Lütfen tekrar giriş yapın.");
        }

        // 3. Yeni Tenant Token Üret (AccessToken)
        string newAccessToken = await jwtProvider.CreateTenantTokenAsync(
            user,
            existingToken.CompanyUser, // Rolleri buradan alacak
            cancellationToken);

        // 4. Token Rotation (RefreshToken)
        var newRefreshToken = jwtProvider.CreateRefreshToken();

        existingToken.RevokedAt = DateTimeOffset.Now;
        existingToken.ReplacedByToken = newRefreshToken;

        user.AddRefreshToken(new UserRefreshToken
        {
            Code = newRefreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            CompanyUserId = existingToken.CompanyUserId, // Aynı şirketten devam
            UserId = user.Id
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}