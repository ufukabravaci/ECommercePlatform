using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Products;

public class UpdateProductViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = default!;

    [Display(Name = "Açıklama")]
    public string Description { get; set; } = default!;

    [Required]
    public decimal PriceAmount { get; set; }

    [Required]
    public string CurrencyCode { get; set; } = default!;

    [Required]
    public int Stock { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    // Dropdown verileri
    [ValidateNever]
    public List<SelectListItem>? CategoryList { get; set; }

    [ValidateNever]
    public List<SelectListItem>? CurrencyList { get; set; }
}