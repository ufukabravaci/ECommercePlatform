using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models;

public record ConfirmEmailViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Onay kodu zorunludur.")]
    [Display(Name = "Onay Kodu")]
    public string Code { get; set; } = default!;
}
