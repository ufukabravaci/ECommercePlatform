namespace ECommercePlatform.MvcAdmin.Models.Employee;

public class UpdatePermissionsBatchViewModel
{
    public Guid UserId { get; set; }
    public List<PermissionChangeItem> Changes { get; set; } = new();
}

public class PermissionChangeItem
{
    public string Permission { get; set; } = default!;
    public bool IsGranted { get; set; }
}