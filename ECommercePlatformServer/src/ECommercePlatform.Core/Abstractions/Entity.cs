namespace ECommercePlatform.Domain.Abstractions;

public abstract class Entity : IAuditableEntity
{
    //user dışındaki classlar buradan türeyecek.
    protected Entity()
    {
        //sıralı Guid (Sortable)
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.Now;
        IsActive = true;
        IsDeleted = false;
    }

    public Guid Id { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public void SetStatus(bool isActive) => IsActive = isActive;

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.Now;
    }
}