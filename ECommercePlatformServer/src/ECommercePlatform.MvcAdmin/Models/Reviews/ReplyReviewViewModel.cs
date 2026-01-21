using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Reviews;

public class ReplyReviewViewModel
{
    [Required]
    public Guid ReviewId { get; set; }

    public Guid? ProductId { get; set; }

    [Required(ErrorMessage = "Yanıt metni zorunludur.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yanıt 10-1000 karakter arasında olmalıdır.")]
    [Display(Name = "Yanıtınız")]
    public string Reply { get; set; } = default!;
}