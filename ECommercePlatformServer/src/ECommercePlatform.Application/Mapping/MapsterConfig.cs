using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Banners;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Reviews;
using Mapster;

namespace ECommercePlatform.Application.Mapping;

public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Company -> CompanyDto Eşleştirmesi
        config.NewConfig<Company, CompanyDto>()
            .Map(dest => dest.City, src => src.Address != null ? src.Address.City : "")
            .Map(dest => dest.District, src => src.Address != null ? src.Address.District : "")
            .Map(dest => dest.Street, src => src.Address != null ? src.Address.Street : "")
            .Map(dest => dest.FullAddress, src => src.Address != null ? src.Address.FullAddress : "")
            // Diğer alanlar (Id, Name, TaxNumber) isimleri aynı olduğu için otomatik eşleşir.
            .RequireDestinationMemberSource(true); // Hata yaparsak derleme anında uyarsın

        config.NewConfig<Category, CategoryDto>()
            .Map(dest => dest.ParentName, src => src.Parent != null ? src.Parent.Name : "-")
            // Entity içinde Id, Name, Slug, ParentId zaten public property olduğu için otomatik eşleşir.
            .RequireDestinationMemberSource(true);

        config.NewConfig<Order, OrderListDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.TotalAmount, src => src.Items.Sum(x => x.Price.Amount * x.Quantity))
            .Map(dest => dest.ItemCount, src => src.Items.Count)
            // SQL Translation için açıkça string birleştirme yapıyoruz:
            .Map(dest => dest.CustomerName, src => src.Customer.FirstName + " " + src.Customer.LastName)
            .RequireDestinationMemberSource(true);

        config.NewConfig<OrderItem, OrderItemDto>()
            .Map(dest => dest.PriceAmount, src => src.Price.Amount)
            .Map(dest => dest.PriceCurrency, src => src.Price.Currency)
            .Map(dest => dest.Total, src => src.Price.Amount * src.Quantity);

        config.NewConfig<Order, OrderDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.TotalAmount, src => src.Items.Sum(x => x.Price.Amount * x.Quantity))
            // Adres Value Object Flattening
            .Map(dest => dest.ShippingCity, src => src.ShippingAddress.City)
            .Map(dest => dest.ShippingDistrict, src => src.ShippingAddress.District)
            .Map(dest => dest.ShippingStreet, src => src.ShippingAddress.Street)
            .Map(dest => dest.ShippingZipCode, src => src.ShippingAddress.ZipCode)
            .Map(dest => dest.ShippingFullAddress, src => src.ShippingAddress.FullAddress)
            // Items listesini Mapster otomatik olarak OrderItemDto config'ini kullanarak mapler.
            .Map(dest => dest.Items, src => src.Items);

        config.NewConfig<Review, ReviewDto>()
           .Map(dest => dest.CustomerName, src => src.Customer.FirstName + " " + src.Customer.LastName)
           .RequireDestinationMemberSource(true);

        config.NewConfig<Brand, BrandDto>();
        config.NewConfig<Banner, BannerDto>();

        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.BrandName, src => src.Brand.Name)
            .Map(dest => dest.MainImageUrl, src => src.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault())
            .RequireDestinationMemberSource(true);

        // DashboardRecentOrderDto Eşleştirmesi
        config.NewConfig<Order, DashboardRecentOrderDto>()
            .Map(dest => dest.CustomerName, src => src.Customer != null
                ? src.Customer.FirstName + " " + src.Customer.LastName
                : "Bilinmiyor")
            .Map(dest => dest.TotalAmount, src => src.Items.Sum(i => i.Price.Amount * i.Quantity))
            .Map(dest => dest.CurrencyCode, src => src.Items.AsQueryable().Select(i => i.Price.Currency.ToString()).FirstOrDefault() ?? "TRY")
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        // DashboardLowStockProductDto Eşleştirmesi
        config.NewConfig<Product, DashboardLowStockProductDto>()
            .Map(dest => dest.ImageUrl, src => src.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault());
    }
}
