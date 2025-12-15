namespace ECommercePlatform.Domain.Abstractions;

public interface IMultiTenantEntity
{
    Guid CompanyId { get; }
}
