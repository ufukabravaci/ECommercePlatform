using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.DTOs.Products;

namespace ECommercePlatform.MvcAdmin.Models.Products;

public class ProductListViewModel
{
    public PageResult<ProductDto> Products { get; set; } = new();
}