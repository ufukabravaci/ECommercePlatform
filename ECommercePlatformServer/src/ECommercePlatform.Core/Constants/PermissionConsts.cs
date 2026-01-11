namespace ECommercePlatform.Domain.Constants;

public static class PermissionConsts
{
    // Company Permissions
    public const string UpdateCompany = "Company.Update";
    public const string DeleteCompany = "Company.Delete";
    public const string ReadCompany = "Company.Read";
    public const string UpdateShippingSettings = "Company.UpdateShippingSettings";

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

    // Banner Permissions
    public const string CreateBanner = "Banner.Create";
    public const string UpdateBanner = "Banner.Update";
    public const string DeleteBanner = "Banner.Delete";
    public const string ReadBanner = "Banner.Read";

    // Review Permissions
    public const string CreateReview = "Review.Create";
    public const string ManageReview = "Review.Manage";
    public const string ReadReview = "Review.Read";

    //Customer Permissions
    public const string ReadCustomer = "Customer.Read";
    public const string DeleteCustomer = "Customer.Delete";
}
