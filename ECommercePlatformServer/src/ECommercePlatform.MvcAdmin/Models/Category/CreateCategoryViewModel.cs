using ECommercePlatform.MvcAdmin.DTOs.Category;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.MvcAdmin.Models.Category;

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 100 karakter arasında olmalıdır.")]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = default!;

    [Display(Name = "Üst Kategori")]
    public Guid? ParentId { get; set; }

    [ValidateNever]
    public List<CategoryDto>? AvailableCategories { get; set; }

    [ValidateNever]
    public List<CategoryTreeItem>? CategoryTree { get; set; }
}
