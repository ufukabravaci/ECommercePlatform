using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Customers;

[Permission(PermissionConsts.DeleteCustomer)]
public sealed record RemoveCustomerCommand(Guid CustomerId) : IRequest<Result<string>>;

public sealed class RemoveCustomerCommandHandler(
    IRepository<CompanyUser> companyUserRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext
) : IRequestHandler<RemoveCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RemoveCustomerCommand request, CancellationToken cancellationToken)
    {
        // Şirket ile Kullanıcı arasındaki bağı bul
        var companyUser = await companyUserRepository.GetByExpressionWithTrackingAsync(
            x => x.UserId == request.CustomerId && x.CompanyId == tenantContext.CompanyId,
            cancellationToken);

        if (companyUser is null)
            return Result<string>.Failure("Müşteri bulunamadı.");

        companyUserRepository.Delete(companyUser);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Müşteri şirketinizden çıkarıldı.");
    }
}