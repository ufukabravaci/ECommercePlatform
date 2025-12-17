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
        RuleFor(p => p.Email).EmailAddress().NotEmpty();
        RuleFor(p => p.Password).NotEmpty().MinimumLength(6);
        RuleFor(p => p.ConfirmPassword).Equal(p => p.Password).WithMessage("Şifreler uyuşmuyor.");
        RuleFor(p => p.CompanyName).NotEmpty();
        RuleFor(p => p.TaxNumber).NotEmpty().Length(10, 11).WithMessage("Vergi numarası 10-11 haneli olmalıdır.");
    }
}

public sealed class RegisterTenantCommandHandler(
UserManager<User> userManager,
ICompanyRepository companyRepository,
IUnitOfWork unitOfWork,
IEmailService mailService,
ILogger<RegisterTenantCommandHandler> logger
) : IRequestHandler<RegisterTenantCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Company var mı?
        if (await companyRepository.AnyAsync(c => c.TaxNumber == request.TaxNumber, cancellationToken))
            return Result<string>.Failure("Bu vergi numarası ile kayıtlı şirket bulunmaktadır.");

        // 2. Email var mı?
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result<string>.Failure("Bu email adresi zaten kullanılmaktadır.");

        User? user = null;
        Company? company = null;

        try
        {
            // 3. User oluştur
            user = new User(request.FirstName, request.LastName, request.Email, request.Email);

            var createUserResult = await userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
                return Result<string>.Failure(createUserResult.Errors.Select(x => x.Description).ToList());

            // 4. Company oluştur
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

            // 5. Usera Company bağla
            user.AssignCompany(company.Id);

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
                throw new ApplicationException("Kullanıcı şirkete bağlanamadı.");

            // 6. Role ata

            var roleResult = await userManager.AddToRoleAsync(user, RoleConsts.CompanyOwner);
            if (!roleResult.Succeeded)
                throw new ApplicationException("Rol ataması başarısız.");

            // 7. Email gönder
            var otpCode = await userManager.GenerateEmailConfirmationTokenAsync(user);

            await mailService.SendAsync(
                user.Email!,
                "E-Ticaret Platformu - Kayıt Onayı",
                $"<b>Doğrulama Kodunuz:</b> {otpCode}",
                cancellationToken
            );

            return "Kayıt başarılı. Email doğrulaması bekleniyor.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterTenant failed. Email: {Email}", request.Email);

            // CLEANUP
            if (user is not null)
            {
                user.Delete();
                await userManager.UpdateAsync(user);
            }

            if (company is not null)
            {
                company.Delete(); // IsDeleted = true
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<string>.Failure("Kayıt sırasında beklenmeyen bir hata oluştu.");
        }
    }
}