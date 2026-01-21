using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Banners;

public class UpdateBannerViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = default!;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = default!;

    [Display(Name = "Yeni Görsel (Opsiyonel)")]
    public IFormFile? Image { get; set; }

    [Required(ErrorMessage = "Hedef URL zorunludur.")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    [Display(Name = "Hedef URL")]
    public string TargetUrl { get; set; } = default!;

    [Required(ErrorMessage = "Sıralama zorunludur.")]
    [Range(1, 100, ErrorMessage = "Sıralama 1-100 arasında olmalıdır.")]
    [Display(Name = "Sıralama")]
    public int Order { get; set; }

    // Mevcut görsel URL'i (gösterim için)
    public string? CurrentImageUrl { get; set; }
}