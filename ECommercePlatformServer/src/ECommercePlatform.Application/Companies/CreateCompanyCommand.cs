//Burası sadece superadmin yeni bir şirket oluşturduğunda çalışacak

using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;

public sealed record CreateCompanyCommand(
    string Name,
    string TaxNumber,
    string? City,
    string? District,
    string? Street,
    string? ZipCode,
    string? FullAddress
) : IRequest<Result<string>>;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Şirket adı boş olamaz.");
        RuleFor(x => x.TaxNumber).NotEmpty().Length(10, 11).WithMessage("Vergi numarası 10 veya 11 hane olmalıdır.");
    }
}

public sealed class CreateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository, // User'ı güncellemek için
    IUserContext userContext, // Şu anki kullanıcıyı bulmak için
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCompanyCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        //şirket zaten kayıtlı mı kontrol et
        var isExists = await companyRepository
            .AnyAsync(c => c.TaxNumber == request.TaxNumber, cancellationToken);
        if (isExists)
        {
            return Result<string>.Failure("Bu vergi numarası ile zaten bir şirket kayıtlı.");
        }

        //şirket kayıtlı değil, kullanıcıyı bul
        var userId = userContext.GetUserId();
        if (userId == Guid.Empty) return Result<string>.Failure("Kullanıcı bulunamadı.");
        var user = await userRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı.");

        //kullanıcı bulundu, yeni şirket oluştur
        var company = new Company(request.Name, request.TaxNumber);

        //adres bilgisi varsa ekle
        if (!string.IsNullOrWhiteSpace(request.City) && !string.IsNullOrWhiteSpace(request.Street))
        {
            company.UpdateAddress(new Address(
                request.City,
                request.District ?? "",
                request.Street,
                request.ZipCode ?? "",
                request.FullAddress ?? ""
            ));
        }

        companyRepository.Add(company);
        user.AssignCompany(company.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return "Şirket başarıyla oluşturuldu.";
    }
}
