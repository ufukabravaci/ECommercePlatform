using ECommercePlatform.Application.Categories;
using ECommercePlatform.Application.Companies;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
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
    }
}
