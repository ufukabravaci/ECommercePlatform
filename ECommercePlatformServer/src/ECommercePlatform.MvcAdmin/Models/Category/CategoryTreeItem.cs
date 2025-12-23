namespace ECommercePlatform.MvcAdmin.Models.Category;


public class CategoryTreeItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int Level { get; set; }
}
