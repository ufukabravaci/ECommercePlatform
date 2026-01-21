namespace ECommercePlatform.Domain.Constants;

public static class PermissionMetadata
{
    public static readonly Dictionary<string, (string Label, string Icon)> Groups = new()
    {
        { "Company", ("Şirket", "fa-building") },
        { "Category", ("Kategori", "fa-folder") },
        { "Product", ("Ürün", "fa-box") },
        { "Order", ("Sipariş", "fa-shopping-cart") },
        { "Brand", ("Marka", "fa-tag") },
        { "Banner", ("Banner", "fa-image") },
        { "Review", ("Yorum", "fa-star") },
        { "Customer", ("Müşteri", "fa-users") },
        { "Employee", ("Çalışan", "fa-user-tie") }
    };

    public static (string Label, string Icon) GetGroupInfo(string groupName)
    {
        return Groups.TryGetValue(groupName, out var info) ? info : (groupName, "fa-shield-alt");
    }
}
