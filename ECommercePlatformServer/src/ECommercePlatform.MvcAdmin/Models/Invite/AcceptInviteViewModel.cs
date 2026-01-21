using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Invite;

public class AcceptInviteViewModel
{
    public string Token { get; set; } = default!;
    public string? InvitedEmail { get; set; }
    public string? CompanyName { get; set; }
    public string? Role { get; set; }

    // Aktif Tab: "register" veya "login"
    public string ActiveTab { get; set; } = "register";

    // Tab A - Yeni Kayıt
    public RegisterEmployeeFormModel RegisterForm { get; set; } = new();

    // Tab B - Mevcut Hesap ile Giriş
    public LoginAndAcceptFormModel LoginForm { get; set; } = new();
}

public class RegisterEmployeeFormModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MinLength(2, ErrorMessage = "Ad en az 2 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MinLength(2, ErrorMessage = "Soyad en az 2 karakter olmalıdır.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = default!;
}

public class LoginAndAcceptFormModel
{
    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = default!;
}