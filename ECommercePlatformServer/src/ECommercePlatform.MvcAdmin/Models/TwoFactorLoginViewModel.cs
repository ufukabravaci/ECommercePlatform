using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models;


public record TwoFactorLoginViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
    [Display(Name = "Doğrulama Kodu")]
    public string Code { get; set; } = default!;
}