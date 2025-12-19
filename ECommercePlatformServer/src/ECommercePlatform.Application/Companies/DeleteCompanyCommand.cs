using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;

[Permission(PermissionConsts.DeleteCompany)]
public sealed record DeleteCompanyCommand() : IRequest<Result<string>>;

public sealed class DeleteCompanyCommandHandler(
    ICompanyRepository companyRepository,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteCompanyCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyId = tenantContext.GetCompanyId();
        if (companyId is null) return Result<string>.Failure("Şirket bulunamadı.");

        var company = await companyRepository.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company is null) return Result<string>.Failure("Şirket bulunamadı.");

        // Entity sınıfımız IAuditableEntity implemente ettiği için Delete() metodu var. Soft delete yapar.
        company.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şirket hesabı başarıyla kapatıldı (Silindi).";
    }
}