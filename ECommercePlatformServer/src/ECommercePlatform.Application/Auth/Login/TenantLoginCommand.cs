using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Login;

public sealed record TenantLoginCommand(string EmailOrUserName, string Password) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    bool RequiresEmailConfirmation,
    string? Message);

public sealed class TenantLoginCommandValidator : AbstractValidator<TenantLoginCommand>
{
    public TenantLoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty().WithMessage("Email veya kullanıcı adı boş olamaz.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre boş olamaz.");
    }
}

public sealed class TenantLoginCommandHandler(
    UserManager<User> userManager,
    IJwtProvider jwtProvider,
    IEmailService mailService,
    IUserRefreshTokenRepository refreshTokenRepository,
    ITenantContext tenantContext,
    ICompanyUserRepository companyUserRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<TenantLoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        TenantLoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1️⃣ User bul
        User? user = request.EmailOrUserName.Contains("@")
                ? await userManager.FindByEmailAsync(request.EmailOrUserName)
                : await userManager.FindByNameAsync(request.EmailOrUserName);

        if (user is null)
            return Result<LoginResponse>.Failure("Kullanıcı adı veya şifre hatalı.");

        // 2️⃣ Şifre Kontrol
        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<LoginResponse>.Failure("Kullanıcı adı veya şifre hatalı.");
        }
        await userManager.ResetAccessFailedCountAsync(user);

        // 3️⃣ ŞİRKET SEÇİMİ (AKILLI LOGIC)
        Guid targetCompanyId;

        // A. Header'dan ID gelmiş mi? (Frontend biliyorsa gönderir)
        if (tenantContext.CompanyId.HasValue)
        {
            targetCompanyId = tenantContext.CompanyId.Value;
        }
        // B. Header yok, veritabanına bak
        else
        {
            // Kullanıcının üye olduğu tüm şirketleri çek (Sadece ID'ler yeterli)
            var userCompanyIds = await companyUserRepository.GetAll()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.CompanyId)
                .ToListAsync(cancellationToken);

            if (userCompanyIds.Count == 0)
                return Result<LoginResponse>.Failure("Bağlı olduğunuz bir şirket bulunamadı.");

            if (userCompanyIds.Count == 1)
            {
                // Tek şirketi var, otomatik seç
                targetCompanyId = userCompanyIds.First();
            }
            else
            {
                // Birden fazla şirketi var ve Header göndermemiş
                // (Gelişmiş senaryoda burada şirket listesi dönülebilir)
                return Result<LoginResponse>.Failure("Birden fazla şirketiniz var. Lütfen giriş yapılacak şirketi seçiniz.");
            }
        }

        // 4️⃣ Yetki Kontrolü (User + TargetCompanyId)
        // Token üretmek için CompanyUser nesnesinin tamamına (Rollerine) ihtiyacımız var.
        var companyUser = await companyUserRepository.FirstOrDefaultAsync(
            x => x.UserId == user.Id && x.CompanyId == targetCompanyId,
            cancellationToken);

        if (companyUser is null)
            return Result<LoginResponse>.Failure("Seçilen şirkete giriş yetkiniz yok.");

        // 5️⃣ Lockout Kontrolü
        if (await userManager.IsLockedOutAsync(user))
        {
            var end = await userManager.GetLockoutEndDateAsync(user);
            var remaining = end!.Value - DateTimeOffset.Now;
            return Result<LoginResponse>.Failure(
                $"Hesabınız kilitli. {Math.Ceiling(remaining.TotalMinutes)} dakika sonra deneyin.");
        }

        // 6️⃣ Email confirmation
        //if (!user.EmailConfirmed)
        //{
        //    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        //    await mailService.SendAsync(
        //        user.Email!,
        //        "Email Doğrulama",
        //        $"Doğrulama kodunuz: <b>{code}</b>",
        //        cancellationToken);

        //    return new LoginResponse(null, null, false, true, "Email doğrulaması gerekli.");
        //}

        // 7️⃣ 2FA
        //if (user.TwoFactorEnabled)
        //{
        //    var code = await userManager.GenerateTwoFactorTokenAsync(user, "SixDigit");
        //    await mailService.SendAsync(
        //        user.Email!,
        //        "2FA Doğrulama",
        //        $"Doğrulama kodunuz: <b>{code}</b>",
        //        cancellationToken);

        //    return new LoginResponse(null, null, true, false, "2FA doğrulaması gerekli.");
        //}

        // 8️⃣ Token Üretimi (CreateTenantTokenAsync)
        var accessToken = await jwtProvider.CreateTenantTokenAsync(
                user,
                companyUser, // Rolleri buradan alacak
                cancellationToken);

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

        return new LoginResponse(accessToken, refreshToken, false, false, "Giriş başarılı.");
    }
}