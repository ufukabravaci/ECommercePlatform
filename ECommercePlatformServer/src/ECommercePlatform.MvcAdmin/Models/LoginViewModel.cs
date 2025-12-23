using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models;

public record LoginViewModel
{
    [Required(ErrorMessage = "Email veya kullanıcı adı zorunludur.")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email veya Kullanıcı Adı")]
    public string EmailOrUserName { get; set; } = default!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = default!;
}