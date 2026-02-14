using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Products;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "SKU zorunludur.")]
    public string Sku { get; set; } = default!;

    [Display(Name = "Açıklama")]
    [MaxLength(2000)]
    public string Description { get; set; } = default!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal PriceAmount { get; set; }

    [Required]
    [Display(Name = "Para Birimi")]
    public string CurrencyCode { get; set; } = "TRY";

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Display(Name = "Kategori")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Marka seçimi zorunludur.")]
    [Display(Name = "Marka")]
    public Guid BrandId { get; set; }

    [Display(Name = "Ürün Resimleri (Max 5)")]
    public List<IFormFile>? Files { get; set; }

    // Dropdown verileri
    [ValidateNever]
    public List<SelectListItem>? CategoryList { get; set; }

    [ValidateNever]
    public List<SelectListItem>? CurrencyList { get; set; }

    [ValidateNever]
    public List<SelectListItem>? BrandList { get; set; }
}