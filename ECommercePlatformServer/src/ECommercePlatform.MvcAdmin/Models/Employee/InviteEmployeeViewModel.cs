using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Employee;


public class InviteEmployeeViewModel
{
    [Required(ErrorMessage = "Email adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Rol seçimi zorunludur.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = default!;
}
