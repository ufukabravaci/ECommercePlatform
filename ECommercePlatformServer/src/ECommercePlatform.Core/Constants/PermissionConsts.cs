namespace ECommercePlatform.Domain.Constants;

public static class PermissionConsts
{
    // Company Permissions
    public const string UpdateCompany = "Company.Update";
    public const string DeleteCompany = "Company.Delete";
    public const string ReadCompany = "Company.Read";

    // Category Permissions
    public const string CreateCategory = "Category.Create";
    public const string UpdateCategory = "Category.Update";
    public const string DeleteCategory = "Category.Delete";
    public const string ReadCategory = "Category.Read";

    // Product Permissions
    public const string CreateProduct = "Product.Create";
    public const string UpdateProduct = "Product.Update";
    public const string DeleteProduct = "Product.Delete";
    public const string ReadProduct = "Product.Read";
    public const string ManageProductImages = "Product.ManageImages";

    // Order Permissions
    public const string CreateOrder = "Order.Create";
    public const string ReadOrder = "Order.Read";
    public const string ReadAllOrders = "Order.Read.All";
    public const string UpdateOrderStatus = "Order.UpdateStatus";

    // Brand Permissions
    public const string CreateBrand = "Brand.Create";
    public const string UpdateBrand = "Brand.Update";
    public const string DeleteBrand = "Brand.Delete";
    public const string ReadBrand = "Brand.Read";
}
