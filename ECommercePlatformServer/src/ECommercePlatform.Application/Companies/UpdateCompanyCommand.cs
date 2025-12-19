using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;

[Permission(PermissionConsts.UpdateCompany)]
public sealed record UpdateCompanyCommand(
    string Name,
    string TaxNumber,
    string City,
    string District,
    string Street,
    string ZipCode,
    string FullAddress
) : IRequest<Result<string>>;

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Şirket adı boş olamaz."); ;
        RuleFor(p => p.TaxNumber).NotEmpty().WithMessage("Vergi numarası boş olamaz.")
            .Length(10, 11).WithMessage("Vergi numarası 10 veya 11 hane olmalıdır."); ;
    }
}

public sealed class UpdateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateCompanyCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyId = tenantContext.GetCompanyId();
        if (companyId is null) return Result<string>.Failure("Şirket bulunamadı.");

        var company = await companyRepository.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company is null) return Result<string>.Failure("Şirket bulunamadı.");

        // Vergi numarası değişiyorsa, başkasının numarasını almadığından emin ol
        if (company.TaxNumber != request.TaxNumber)
        {
            var isTaxExists = await companyRepository.AnyAsync(c => c.TaxNumber == request.TaxNumber && c.Id != companyId, cancellationToken);
            if (isTaxExists) return Result<string>.Failure("Bu vergi numarası başka bir şirket tarafından kullanılıyor.");
        }

        // Güncelleme (Domain Metotları ile)
        company.UpdateName(request.Name);
        company.UpdateTaxNumber(request.TaxNumber);

        var newAddress = new Address(
            request.City,
            request.District ?? "",
            request.Street ?? "",
            request.ZipCode ?? "",
            request.FullAddress ?? ""
        );
        company.UpdateAddress(newAddress);

        companyRepository.Update(company);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şirket bilgileri güncellendi.";
    }
}