using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Login;

public sealed record TenantLoginWithTwoFactorCommand(
    string Email,
    string Code,
    Guid CompanyId
) : IRequest<Result<TenantLoginWithTwoFactorResponse>>;

public sealed record TenantLoginWithTwoFactorResponse(
    string AccessToken,
    string RefreshToken
);

public sealed class TenantLoginWithTwoFactorCommandValidator
    : AbstractValidator<TenantLoginWithTwoFactorCommand>
{
    public TenantLoginWithTwoFactorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.CompanyId)
            .NotEmpty();
    }
}
public sealed class TenantLoginWithTwoFactorCommandHandler(
    UserManager<User> userManager,
    ICompanyUserRepository companyUserRepository,
    IJwtProvider jwtProvider,
    IUserRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork
)
: IRequestHandler<TenantLoginWithTwoFactorCommand, Result<TenantLoginWithTwoFactorResponse>>
{
    public async Task<Result<TenantLoginWithTwoFactorResponse>> Handle(
        TenantLoginWithTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // 1️⃣ User var mı?
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<TenantLoginWithTwoFactorResponse>
                .Failure("Kullanıcı bulunamadı.");

        // 2️⃣ 2FA doğrulama
        var isValid2Fa = await userManager.VerifyTwoFactorTokenAsync(
            user,
            "SixDigit",
            request.Code);

        if (!isValid2Fa)
            return Result<TenantLoginWithTwoFactorResponse>
                .Failure("Geçersiz doğrulama kodu.");

        // 3️⃣ CompanyUser kontrolü (EN KRİTİK NOKTA)
        var companyUser = await companyUserRepository
            .FirstOrDefaultAsync(
                x => x.UserId == user.Id && x.CompanyId == request.CompanyId,
                cancellationToken);

        if (companyUser is null)
            return Result<TenantLoginWithTwoFactorResponse>
                .Failure("Bu şirkete ait kullanıcı kaydı bulunamadı.");

        // 4️⃣ TENANT ACCESS TOKEN
        var accessToken = await jwtProvider.CreateTenantTokenAsync(
            user,
            companyUser,
            cancellationToken);

        // 5️⃣ TENANT REFRESH TOKEN
        var refreshToken = jwtProvider.CreateRefreshToken();

        var refreshTokenEntity = new UserRefreshToken
        {
            UserId = user.Id,
            Code = refreshToken,
            Expiration = DateTimeOffset.Now.AddDays(7),
            CompanyUserId = companyUser.Id
        };

        refreshTokenRepository.Add(refreshTokenEntity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantLoginWithTwoFactorResponse(
            accessToken,
            refreshToken);
    }
}

