using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Brands;

public class UpdateBrandViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Marka adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Marka adı 2-100 karakter arasında olmalıdır.")]
    [Display(Name = "Marka Adı")]
    public string Name { get; set; } = default!;

    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    [Display(Name = "Logo URL")]
    public string? LogoUrl { get; set; }
}