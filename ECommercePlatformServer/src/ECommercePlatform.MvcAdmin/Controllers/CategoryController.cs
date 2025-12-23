using ECommercePlatform.MvcAdmin.DTOs.Category;
using ECommercePlatform.MvcAdmin.Models;
using ECommercePlatform.MvcAdmin.Models.Category;
using ECommercePlatform.MvcAdmin.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class CategoryController : Controller
{
    private readonly IApiService _apiService;

    public CategoryController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // LIST (GET)
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _apiService.GetAsync<List<CategoryDto>>("api/categories");

        if (!result.IsSuccessful)
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Kategoriler yüklenemedi.";
            return View(PaginatedList<CategoryDto>.Create(new List<CategoryDto>(), 1, pageSize));
        }

        var categories = result.Data ?? new List<CategoryDto>();

        // Tree için tüm kategoriler lazım
        ViewBag.CategoryTree = BuildCategoryTree(categories);

        // Sayfalama uygula
        var paginatedCategories = PaginatedList<CategoryDto>.Create(categories, page, pageSize);

        return View(paginatedCategories);
    }

    // CREATE (GET) - Formu Göster
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await GetCategoriesForDropdown();

        var model = new CreateCategoryViewModel
        {
            AvailableCategories = categories,
            CategoryTree = BuildCategoryTree(categories)
        };
        return View(model);
    }

    // CREATE (POST) - Formu Gönder
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await GetCategoriesForDropdown();
            model.AvailableCategories = categories;
            model.CategoryTree = BuildCategoryTree(categories);
            return View(model);
        }

        var requestDto = model.Adapt<CreateCategoryRequestDto>();
        var result = await _apiService.PostAsync<string>("api/categories", requestDto);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data;
            return RedirectToAction(nameof(Index));
        }

        // API hatası - CategoryTree'yi yeniden yükle
        ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Oluşturma başarısız.";
        var cats = await GetCategoriesForDropdown();
        model.AvailableCategories = cats;
        model.CategoryTree = BuildCategoryTree(cats);  // ✅ ÖNEMLİ
        return View(model);
    }

    // EDIT (GET) - Düzenleme Formunu Göster
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var categoriesResult = await _apiService.GetAsync<List<CategoryDto>>("api/categories");

        if (!categoriesResult.IsSuccessful || categoriesResult.Data is null)
        {
            TempData["ErrorMessage"] = "Kategori bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var allCategories = categoriesResult.Data;
        var categoryToEdit = allCategories.FirstOrDefault(c => c.Id == id);

        if (categoryToEdit is null)
        {
            return NotFound();
        }

        var model = new UpdateCategoryViewModel
        {
            Id = categoryToEdit.Id,
            Name = categoryToEdit.Name,
            ParentId = categoryToEdit.ParentId,
            AvailableCategories = allCategories.Where(c => c.Id != id).ToList(),
            // Kendisi ve potansiyel olarak kendi alt kategorileri hariç
            CategoryTree = BuildCategoryTree(allCategories, excludeId: id)
        };

        return View(model);
    }

    // EDIT (POST) - Güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCategoryViewModel model)
    {
        // Model validasyon hatası
        if (!ModelState.IsValid)
        {
            var categories = await GetCategoriesForDropdown(excludeId: model.Id);
            model.AvailableCategories = categories;
            model.CategoryTree = BuildCategoryTree(categories, excludeId: model.Id);
            return View(model);
        }

        var requestDto = model.Adapt<UpdateCategoryRequestDto>();
        var result = await _apiService.PutAsync<string>("api/categories", requestDto);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data;
            return RedirectToAction(nameof(Index));
        }

        // API hatası - CategoryTree'yi yeniden yükle
        ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Güncelleme başarısız.";
        var cats = await GetCategoriesForDropdown(excludeId: model.Id);
        model.AvailableCategories = cats;
        model.CategoryTree = BuildCategoryTree(cats, excludeId: model.Id);  // ✅ ÖNEMLİ
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        // API'de GetById olmadığı için GetAll'dan filtreliyoruz
        var categoriesResult = await _apiService.GetAsync<List<CategoryDto>>("api/categories");

        if (!categoriesResult.IsSuccessful || categoriesResult.Data is null)
        {
            TempData["ErrorMessage"] = "Kategori bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var category = categoriesResult.Data.FirstOrDefault(c => c.Id == id);

        if (category is null)
        {
            TempData["ErrorMessage"] = "Kategori bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    // DELETE (POST)
    // Genelde Admin panellerinde SweetAlert ile onay alınıp bu endpoint çağrılır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        // API: DELETE /api/categories/{id}
        // Not: ApiService'e DeleteAsync eklediğinizi varsayıyorum.
        var result = await _apiService.DeleteAsync<string>($"api/categories/{id}");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data;
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Silme başarısız.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Yardımcı Metot: Dropdown için kategorileri getirme
    private async Task<List<CategoryDto>> GetCategoriesForDropdown(Guid? excludeId = null)
    {
        var result = await _apiService.GetAsync<List<CategoryDto>>("api/categories");
        if (result.IsSuccessful && result.Data != null)
        {
            if (excludeId.HasValue)
            {
                return result.Data.Where(c => c.Id != excludeId.Value).ToList();
            }
            return result.Data;
        }
        return new List<CategoryDto>();
    }

    private List<CategoryTreeItem> BuildCategoryTree(
    List<CategoryDto> categories,
    Guid? parentId = null,
    int level = 0,
    Guid? excludeId = null)
    {
        var result = new List<CategoryTreeItem>();

        var items = categories
            .Where(c => c.ParentId == parentId && c.Id != excludeId)
            .ToList();

        foreach (var item in items)
        {
            result.Add(new CategoryTreeItem
            {
                Id = item.Id,
                Name = item.Name,
                Level = level
            });

            // Recursive: Alt kategorileri de ekle
            result.AddRange(BuildCategoryTree(categories, item.Id, level + 1, excludeId));
        }

        return result;
    }
}
