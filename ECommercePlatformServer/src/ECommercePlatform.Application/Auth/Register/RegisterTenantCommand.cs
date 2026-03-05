using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Register;

public sealed record RegisterTenantCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string CompanyName,
    string TaxNumber,
    string TaxOffice,
    string FullAddress,
    string City,
    string District,
    string Street,
    string ZipCode
) : IRequest<Result<string>>;

public sealed class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.FirstName).MinimumLength(3).WithMessage("Ad alanı en az 3 karakterden oluşmalıdır.");
        RuleFor(x => x.LastName).MinimumLength(3).WithMessage("Soyad alanı en az 3 karakterden oluşmalıdır.");
        RuleFor(x => x.Email).EmailAddress()
            .WithMessage("Geçersiz e-mail adresi.")
            .NotEmpty().WithMessage("E-mail adresi boş olamaz.");
        RuleFor(p => p.Password).NotEmpty().MinimumLength(6).WithMessage("Şifre alanı en az 6 karakterden oluşmalıdır.");
        RuleFor(p => p.ConfirmPassword).Equal(p => p.Password).WithMessage("Şifreler eşleşmiyor.");
        RuleFor(p => p.CompanyName).MinimumLength(3).WithMessage("Şirket adı en az 3 karakterden oluşmalıdır.");
        RuleFor(p => p.TaxNumber).NotEmpty().Length(10, 11).WithMessage("Vergi numarası 10-11 haneli olmalıdır.");
    }
}

public sealed class RegisterTenantCommandHandler(
    UserManager<User> userManager,
    ICompanyRepository companyRepository,
    ICompanyUserRepository companyUserRepository,
    IUnitOfWork unitOfWork,
    IEmailService mailService, // IEmailService'de SendAsync metodu kullanılıyor
    ILogger<RegisterTenantCommandHandler> logger
) : IRequestHandler<RegisterTenantCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Company kontrol (Transaction dışı, hızlı kontrol)
        if (await companyRepository.AnyAsync(
                c => c.TaxNumber == request.TaxNumber,
                cancellationToken))
        {
            return Result<string>.Failure("Bu vergi numarası ile kayıtlı şirket bulunmaktadır.");
        }

        User? user = null;
        string? emailToken = null;
        Guid? createdCompanyId = null; // Mail'de kullanmak üzere ID'yi dışarıya alıyoruz

        // A. BÜYÜK İŞLEM (TRANSACTION) BAŞLANGICI
        using (var transaction = new System.Transactions.TransactionScope(
                   System.Transactions.TransactionScopeOption.Required,
                   new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                   System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // 2. User var mı?
                user = await userManager.FindByEmailAsync(request.Email);

                if (user is null)
                {
                    user = new User(
                        request.FirstName,
                        request.LastName,
                        request.Email,
                        request.Email
                    );

                    var createUserResult = await userManager.CreateAsync(user, request.Password);
                    if (!createUserResult.Succeeded)
                    {
                        return Result<string>.Failure(createUserResult.Errors.Select(x => x.Description).ToList());
                    }
                }

                // 3. Company oluştur
                var company = new Company(request.CompanyName, request.TaxNumber);

                if (!string.IsNullOrWhiteSpace(request.City))
                {
                    var address = new Address(request.City, request.District, request.Street, request.ZipCode, request.FullAddress);
                    company.UpdateAddress(address);
                }

                companyRepository.Add(company);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Mail'de kullanmak üzere atama yapıyoruz
                createdCompanyId = company.Id;

                // 4. CompanyUser oluştur (kritik bağ)
                var companyUser = new CompanyUser(user.Id, company.Id);
                companyUser.AddRole(RoleConsts.CompanyOwner);
                companyUserRepository.Add(companyUser);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Identity Role (platform genel rol)
                if (!await userManager.IsInRoleAsync(user, RoleConsts.CompanyOwner))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, RoleConsts.CompanyOwner);
                    if (!roleResult.Succeeded)
                        return Result<string>.Failure("Rol ataması başarısız oldu.");
                }

                // Mail için token oluştur (ama maili henüz atma!)
                if (!user.EmailConfirmed)
                {
                    emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                }

                // HER ŞEY BAŞARILI OLURSA VERİTABANINA YAZ (COMMIT)
                transaction.Complete();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RegisterTenant failed in database transaction. Email: {Email}", request.Email);
                return Result<string>.Failure("Veritabanı işlemleri sırasında bir hata oluştu ve kayıt iptal edildi.");
            }
        }

        // B. MAİL GÖNDERİMİ (TRANSACTION DIŞINDA YAPILMALI)
        if (user != null && !user.EmailConfirmed && emailToken != null && createdCompanyId != null)
        {
            try
            {
                // HTML formatında şık bir mail içeriği oluşturuyoruz
                string mailBody = $@"
                    <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                        <h2 style='color: #0056b3;'>E-Ticaret Platformuna Hoş Geldiniz!</h2>
                        <p>Sayın {request.FirstName} {request.LastName},</p>
                        <p><strong>{request.CompanyName}</strong> firması için platformumuzda başarıyla hesap oluşturdunuz. Hesabınızı aktifleştirmek için lütfen aşağıdaki doğrulama kodunu kullanın:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #28a745; margin: 20px 0; font-size: 18px; font-weight: bold;'>
                            Doğrulama Kodunuz: {emailToken}
                        </div>

                        <hr style='border: 1px solid #eee; margin: 30px 0;' />

                        <h3 style='color: #0056b3;'>API Entegrasyon Bilgileri (Geliştiriciler İçin)</h3>
                        <p>Sistemi kullanmaya başlamak ve Frontend (Angular) uygulamasını backend'e bağlamak için size özel oluşturulan <strong>Company ID (Tenant ID)</strong> bilginiz aşağıdadır:</p>
                        
                        <div style='background-color: #f4f4f4; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0; font-family: monospace; font-size: 16px;'>
                            {createdCompanyId}
                        </div>

                        <h4>Frontend Uygulamasında Nasıl Kullanılır?</h4>
                        <ul style='margin-bottom: 20px;'>
                            <li>Angular projenizde <code>src/environments/environment.ts</code> dosyasını açın.</li>
                            <li><code>defaultTenantId</code> alanına yukarıdaki ID'yi yapıştırın.</li>
                            <li>Yaptığınız tüm isteklerde HTTP Interceptor (<code>tenantInterceptor</code>) bu ID'yi otomatik olarak <code>tenant-id</code> header'ı ile WebAPI'ye gönderecektir.</li>
                        </ul>
                        
                        <p><em>Bu ID'yi aynı zamanda Admin Panelinizdeki Şirket Ayarları menüsünden de kopyalayabilirsiniz.</em></p>
                        <br/>
                        <p>İyi çalışmalar dileriz,</p>
                        <p><strong>E-Ticaret Platformu Ekibi</strong></p>
                    </div>";

                await mailService.SendAsync(
                    user.Email!,
                    "E-Ticaret Platformu - Kayıt Onayı ve Kurulum Bilgileri",
                    mailBody,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Kullanıcı başarıyla kaydedildi ama mail gönderilemedi. Email: {Email}", request.Email);
                return "Şirket kaydı başarılı ancak doğrulama maili gönderilemedi. Lütfen sistem yöneticisine başvurun.";
            }
        }

        return "Şirket kaydı başarılı. Email doğrulaması ve kurulum bilgileri adresinize gönderildi.";
    }
}
