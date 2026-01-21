using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Company;

public class UpdateShippingSettingsViewModel
{
    [Required(ErrorMessage = "Ücretsiz kargo limiti zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Değer 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Ücretsiz Kargo Limiti (₺)")]
    public decimal FreeShippingThreshold { get; set; }

    [Required(ErrorMessage = "Sabit kargo ücreti zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Değer 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Sabit Kargo Ücreti (₺)")]
    public decimal FlatRate { get; set; }
}
