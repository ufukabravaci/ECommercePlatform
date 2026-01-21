using System.ComponentModel;

namespace ECommercePlatform.Domain.Constants;

public static class PermissionConsts
{
    // --- Company Permissions ---
    [Description("Şirket Bilgilerini Güncelleme")]
    public const string UpdateCompany = "Company.Update";

    [Description("Şirket Silme")]
    public const string DeleteCompany = "Company.Delete";

    [Description("Şirket Bilgilerini Görüntüleme")]
    public const string ReadCompany = "Company.Read";

    [Description("Kargo Ayarlarını Güncelleme")]
    public const string UpdateShippingSettings = "Company.UpdateShippingSettings";


    // --- Category Permissions ---
    [Description("Kategori Oluşturma")]
    public const string CreateCategory = "Category.Create";

    [Description("Kategori Güncelleme")]
    public const string UpdateCategory = "Category.Update";

    [Description("Kategori Silme")]
    public const string DeleteCategory = "Category.Delete";

    [Description("Kategorileri Görüntüleme")]
    public const string ReadCategory = "Category.Read";


    // --- Product Permissions ---
    [Description("Ürün Oluşturma")]
    public const string CreateProduct = "Product.Create";

    [Description("Ürün Güncelleme")]
    public const string UpdateProduct = "Product.Update";

    [Description("Ürün Silme")]
    public const string DeleteProduct = "Product.Delete";

    [Description("Ürünleri Görüntüleme")]
    public const string ReadProduct = "Product.Read";

    [Description("Ürün Görsellerini Yönetme")]
    public const string ManageProductImages = "Product.ManageImages";


    // --- Order Permissions ---
    [Description("Sipariş Oluşturma")]
    public const string CreateOrder = "Order.Create";

    [Description("Sipariş Görüntüleme")]
    public const string ReadOrder = "Order.Read";

    [Description("Tüm Siparişleri Görüntüleme")]
    public const string ReadAllOrders = "Order.Read.All";

    [Description("Sipariş Durumunu Güncelleme")]
    public const string UpdateOrderStatus = "Order.UpdateStatus";


    // --- Brand Permissions ---
    [Description("Marka Oluşturma")]
    public const string CreateBrand = "Brand.Create";

    [Description("Marka Güncelleme")]
    public const string UpdateBrand = "Brand.Update";

    [Description("Marka Silme")]
    public const string DeleteBrand = "Brand.Delete";

    [Description("Markaları Görüntüleme")]
    public const string ReadBrand = "Brand.Read";


    // --- Banner Permissions ---
    [Description("Banner (Reklam) Oluşturma")]
    public const string CreateBanner = "Banner.Create";

    [Description("Banner Güncelleme")]
    public const string UpdateBanner = "Banner.Update";

    [Description("Banner Silme")]
    public const string DeleteBanner = "Banner.Delete";

    [Description("Bannerları Görüntüleme")]
    public const string ReadBanner = "Banner.Read";


    // --- Review Permissions ---
    [Description("Yorum Oluşturma")]
    public const string CreateReview = "Review.Create";

    [Description("Yorum Yönetimi (Onay/Ret)")]
    public const string ManageReview = "Review.Manage";

    [Description("Yorumları Görüntüleme")]
    public const string ReadReview = "Review.Read";


    // --- Customer Permissions ---
    [Description("Müşterileri Görüntüleme")]
    public const string ReadCustomer = "Customer.Read";

    [Description("Müşteri Silme")]
    public const string DeleteCustomer = "Customer.Delete";


    // --- Employee & Permissions ---
    [Description("Çalışan Listesini Görüntüleme")]
    public const string ReadEmployee = "Employee.Read";

    [Description("Yeni Çalışan Davet Etme")]
    public const string InviteEmployee = "Employee.Invite";

    [Description("Çalışan Yetkilerini Yönetme")]
    public const string ManagePermissions = "Employee.ManagePermissions";
}