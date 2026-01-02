using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth.Register;

public sealed record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<Result<string>>;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad sadece harf ve boşluk içerebilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad sadece harf ve boşluk içerebilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(100).WithMessage("E-posta adresi en fazla 100 karakter olabilir.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Geçersiz e-posta formatı.");

        RuleFor(x => x.Password).MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.ConfirmPassword).MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .Equal(x => x.Password).WithMessage("Şifreler uyuşmuyor.");
    }
}

public sealed class RegisterCustomerCommandHandler(
    UserManager<User> userManager,
    ICompanyUserRepository companyUserRepository,
    ITenantContext tenantContext, // Hangi mağazaya kayıt olduğunu buradan alacağız
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // 1. Hangi mağazadayız? (Header'dan gelen X-Tenant-ID)
        if (tenantContext.CompanyId is null)
        {
            return Result<string>.Failure("Mağaza bilgisi bulunamadı (Tenant ID eksik).");
        }

        Guid currentCompanyId = tenantContext.CompanyId.Value;

        // 2. Kullanıcı Global Olarak Var mı?
        var user = await userManager.FindByEmailAsync(request.Email);

        // Senaryo A: Kullanıcı sistemde hiç yok -> Oluştur ve Mağazaya Bağla
        if (user is null)
        {
            user = new User(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Email
            );

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return Result<string>.Failure(createResult.Errors.Select(e => e.Description).ToList());

        }
        else
        {
            // Senaryo B: Kullanıcı sistemde var

            // Kullanıcı bu mağazaya zaten üye mi?
            bool isAlreadyMember = await companyUserRepository.AnyAsync(
                cu => cu.UserId == user.Id && cu.CompanyId == currentCompanyId,
                cancellationToken);

            if (isAlreadyMember)
            {
                return Result<string>.Failure("Bu e-posta adresi ile bu mağazada zaten bir kayıt mevcut. Lütfen giriş yapınız.");
            }

            return Result<string>.Failure("Bu e-posta adresi sistemde kayıtlı. Lütfen giriş yapınız.");
        }

        // 3. Kullanıcıyı Şirkete (CompanyUser) "Customer" olarak bağla
        var companyUser = new CompanyUser(user.Id, currentCompanyId);
        companyUser.AddRole(RoleConsts.Customer); // Customer Rolü

        companyUserRepository.Add(companyUser);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Üyelik kaydı başarıyla oluşturuldu.");
    }
}
