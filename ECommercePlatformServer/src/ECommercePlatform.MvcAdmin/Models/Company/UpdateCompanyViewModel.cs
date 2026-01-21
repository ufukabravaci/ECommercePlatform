using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Company;

public class UpdateCompanyViewModel
{
    [Required(ErrorMessage = "Şirket adı zorunludur.")]
    [MinLength(3, ErrorMessage = "Şirket adı en az 3 karakter olmalıdır.")]
    [Display(Name = "Şirket Adı")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Vergi numarası zorunludur.")]
    [StringLength(11, MinimumLength = 10, ErrorMessage = "Vergi numarası 10-11 haneli olmalıdır.")]
    [Display(Name = "Vergi Numarası")]
    public string TaxNumber { get; set; } = default!;

    [Required(ErrorMessage = "İl zorunludur.")]
    [Display(Name = "İl")]
    public string City { get; set; } = default!;

    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [Display(Name = "Sokak/Cadde")]
    public string? Street { get; set; }

    [Display(Name = "Posta Kodu")]
    public string? ZipCode { get; set; }

    [Display(Name = "Tam Adres")]
    public string? FullAddress { get; set; }
}
