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
    IEmailService mailService,
    ILogger<RegisterTenantCommandHandler> logger
) : IRequestHandler<RegisterTenantCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Company kontrol
        if (await companyRepository.AnyAsync(
                c => c.TaxNumber == request.TaxNumber,
                cancellationToken))
        {
            return Result<string>.Failure("Bu vergi numarası ile kayıtlı şirket bulunmaktadır.");
        }

        User? user = null;
        Company? company = null;

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
                    return Result<string>.Failure(
                        createUserResult.Errors.Select(x => x.Description).ToList()
                    );
                }
            }

            // 3. Company oluştur
            company = new Company(request.CompanyName, request.TaxNumber);

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var address = new Address(
                    request.City,
                    request.District,
                    request.Street,
                    request.ZipCode,
                    request.FullAddress
                );

                company.UpdateAddress(address);
            }

            companyRepository.Add(company);
            await unitOfWork.SaveChangesAsync(cancellationToken);

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
                    throw new ApplicationException("Rol ataması başarısız.");
            }

            // 6. Email confirmation
            if (!user.EmailConfirmed)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                await mailService.SendAsync(
                    user.Email!,
                    "E-Ticaret Platformu - Kayıt Onayı",
                    $"<b>Doğrulama Kodunuz:</b> {token}",
                    cancellationToken
                );
            }

            return "Şirket kaydı başarılı. Email doğrulaması bekleniyor.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterTenant failed. Email: {Email}", request.Email);

            // Company sil (user silinmez, başka tenant'larda olabilir)
            if (company is not null)
            {
                company.Delete();
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<string>.Failure("Kayıt sırasında beklenmeyen bir hata oluştu.");
        }
    }
}
