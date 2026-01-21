using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

public sealed record RegisterEmployeeCommand(
    string Token,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
) : IRequest<Result<string>>;

public sealed class RegisterEmployeeCommandValidator : AbstractValidator<RegisterEmployeeCommand>
{
    public RegisterEmployeeCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Davet kodu geçersiz.");
        RuleFor(x => x.FirstName).MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.");
        RuleFor(x => x.LastName).MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.");
        RuleFor(x => x.Password).MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Şifreler uyuşmuyor.");
    }
}

public sealed class RegisterEmployeeCommandHandler(
    UserManager<User> userManager,
    ICompanyInvitationRepository invitationRepository, // Invitation repository'niz
    ICompanyUserRepository companyUserRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterEmployeeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterEmployeeCommand request, CancellationToken cancellationToken)
    {
        // 1. Davet Geçerli mi?
        var invitation = await invitationRepository.FirstOrDefaultAsync(
            x => x.Token == request.Token,
            cancellationToken);

        if (invitation is null)
            return Result<string>.Failure("Geçersiz veya hatalı davet kodu.");

        if (invitation.Status != InvitationStatus.Pending)
            return Result<string>.Failure("Bu davet daha önce kullanılmış.");

        if (DateTimeOffset.Now > invitation.ExpirationDate)
            return Result<string>.Failure("Davet süresi dolmuş.");

        // 2. Kullanıcı Zaten Var mı? (Email davetiyeden geliyor)
        var existingUser = await userManager.FindByEmailAsync(invitation.Email);

        if (existingUser is not null)
        {
            // Kullanıcı zaten varsa "Register" endpointini değil, "Login + AcceptInvitation" akışını kullanmalı.
            // Frontend'e bu durumu belirten özel bir kod dönebilirsiniz.
            return Result<string>.Failure("Bu e-posta adresiyle zaten bir üyelik mevcut. Lütfen giriş yaparak daveti kabul ediniz.");
        }

        // 3. Yeni User Oluştur
        var newUser = new User(
            request.FirstName,
            request.LastName,
            invitation.Email, // Email invitation'dan gelir, güvenlidir.
            invitation.Email
        );

        // Çalışanlar için email'i direkt onaylı yapabiliriz çünkü davet mailine tıkladılar.
        newUser.EmailConfirmed = true;

        var createResult = await userManager.CreateAsync(newUser, request.Password);
        if (!createResult.Succeeded)
            return Result<string>.Failure(createResult.Errors.Select(e => e.Description).ToList());

        // 4. CompanyUser Bağlantısı (Employee Rolü ile)
        var companyUser = new CompanyUser(newUser.Id, invitation.CompanyId);
        companyUser.AddRole(RoleConsts.Employee);

        companyUserRepository.Add(companyUser);

        // 5. Daveti Onayla
        invitation.MarkAsAccepted();
        invitationRepository.Update(invitation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Hesabınız oluşturuldu ve şirkete çalışan olarak eklendiniz. Giriş yapabilirsiniz.");
    }
}