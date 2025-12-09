namespace ECommercePlatform.Domain.Abstractions;

public interface IAuditableEntity
{
    Guid Id { get; }
    bool IsActive { get; }
    DateTimeOffset CreatedAt { get; }
    Guid? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    Guid? DeletedBy { get; }

    void SetStatus(bool isActive);
    void Delete();
}
