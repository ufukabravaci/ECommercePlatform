using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Orders;

public class UpdateTrackingViewModel
{
    [Required(ErrorMessage = "Sipariş numarası zorunludur.")]
    public string OrderNumber { get; set; } = default!;

    [Required(ErrorMessage = "Takip numarası zorunludur.")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Takip numarası 5-50 karakter arasında olmalıdır.")]
    [Display(Name = "Kargo Takip Numarası")]
    public string TrackingNumber { get; set; } = default!;
}
