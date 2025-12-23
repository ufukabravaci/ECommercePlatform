using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models;

public record RegisterCompanyViewModel
{
    // --- USER BİLGİLERİ ---
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MinLength(3, ErrorMessage = "Ad en az 3 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MinLength(3, ErrorMessage = "Soyad en az 3 karakter olmalıdır.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Email adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;

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

    // --- COMPANY BİLGİLERİ ---
    [Required(ErrorMessage = "Şirket adı zorunludur.")]
    [MinLength(3, ErrorMessage = "Şirket adı en az 3 karakter olmalıdır.")]
    [Display(Name = "Şirket Adı")]
    public string CompanyName { get; set; } = default!;

    [Required(ErrorMessage = "Vergi numarası zorunludur.")]
    [StringLength(11, MinimumLength = 10, ErrorMessage = "Vergi numarası 10 veya 11 haneli olmalıdır.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Vergi numarası sadece rakamlardan oluşmalıdır.")]
    [Display(Name = "Vergi No")]
    public string TaxNumber { get; set; } = default!;

    [Display(Name = "Vergi Dairesi")]
    public string TaxOffice { get; set; } = default!;

    // Adres Bilgileri (API'de Address value object nullable idi ama genelde kayıt olurken istenir, zorunlu yapıyorum)
    [Required(ErrorMessage = "Şehir seçimi zorunludur.")]
    [Display(Name = "İl")]
    public string City { get; set; } = default!;

    [Required(ErrorMessage = "İlçe zorunludur.")]
    [Display(Name = "İlçe")]
    public string District { get; set; } = default!;

    [Required(ErrorMessage = "Sokak/Cadde zorunludur.")]
    [Display(Name = "Sokak/Cadde")]
    public string Street { get; set; } = default!;

    [Required(ErrorMessage = "Posta kodu zorunludur.")]
    [Display(Name = "Posta Kodu")]
    public string ZipCode { get; set; } = default!;

    [Required(ErrorMessage = "Tam adres zorunludur.")]
    [Display(Name = "Tam Adres")]
    public string FullAddress { get; set; } = default!;
}
