using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.DTOs.Category;
using ECommercePlatform.MvcAdmin.DTOs.Products;
using ECommercePlatform.MvcAdmin.Models.Products;
using ECommercePlatform.MvcAdmin.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class ProductController : Controller
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;

    public ProductController(IApiService apiService, IConfiguration configuration)
    {
        _apiService = apiService;
        _configuration = configuration;
    }

    // API Base URL'ini al
    private string GetApiBaseUrl()
    {
        return _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
    }

    // LIST
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        string endpoint = $"api/products?pageNumber={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) endpoint += $"&search={search}";

        var result = await _apiService.GetAsync<PageResult<ProductDto>>(endpoint);

        if (!result.IsSuccessful || result.Data is null)
        {
            TempData["ErrorMessage"] = "Ürünler yüklenemedi.";
            return View(new ProductListViewModel());
        }

        // API Base URL'ini ViewBag'e ekle
        ViewBag.ApiBaseUrl = GetApiBaseUrl();

        return View(new ProductListViewModel { Products = result.Data });
    }

    // CREATE (GET)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await GetAllCategoriesAsync();

        var model = new CreateProductViewModel
        {
            CurrencyList = GetCurrencySelectList()
        };

        ViewBag.AllCategories = categories;

        return View(model);
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CurrencyList = GetCurrencySelectList();
            ViewBag.AllCategories = await GetAllCategoriesAsync();
            return View(model);
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Name), nameof(model.Name));
        content.Add(new StringContent(model.Sku), nameof(model.Sku));
        content.Add(new StringContent(model.Description ?? ""), nameof(model.Description));
        content.Add(new StringContent(model.PriceAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(model.PriceAmount));
        content.Add(new StringContent(model.CurrencyCode), nameof(model.CurrencyCode));
        content.Add(new StringContent(model.Stock.ToString()), nameof(model.Stock));
        content.Add(new StringContent(model.CategoryId.ToString()), nameof(model.CategoryId));

        if (model.Files != null)
        {
            foreach (var file in model.Files)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "Files", file.FileName);
            }
        }

        var result = await _apiService.PostMultipartAsync<string>("api/products", content);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();
        model.CurrencyList = GetCurrencySelectList();
        ViewBag.AllCategories = await GetAllCategoriesAsync();
        return View(model);
    }

    // EDIT (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiService.GetAsync<ProductDto>($"api/products/{id}");

        if (!result.IsSuccessful || result.Data is null)
        {
            TempData["ErrorMessage"] = "Ürün bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var product = result.Data;
        var categories = await GetAllCategoriesAsync();

        var model = new UpdateProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            PriceAmount = product.PriceAmount,
            CurrencyCode = product.CurrencyCode,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CurrencyList = GetCurrencySelectList()
        };

        ViewBag.AllCategories = categories;
        ViewBag.Images = product.Images;
        ViewBag.CategoryHierarchy = GetCategoryHierarchy(categories, product.CategoryId);

        // ✅ API Base URL'ini ViewBag'e ekle
        ViewBag.ApiBaseUrl = GetApiBaseUrl();

        return View(model);
    }

    // EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CurrencyList = GetCurrencySelectList();
            ViewBag.AllCategories = await GetAllCategoriesAsync();
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            return View(model);
        }

        var requestDto = model.Adapt<UpdateProductRequestDto>();
        var result = await _apiService.PutAsync<string>("api/products", requestDto);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data;
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();
        model.CurrencyList = GetCurrencySelectList();
        ViewBag.AllCategories = await GetAllCategoriesAsync();
        ViewBag.ApiBaseUrl = GetApiBaseUrl();
        return View(model);
    }

    // --- RESİM İŞLEMLERİ ---

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, bool isMain)
    {
        if (file == null)
        {
            TempData["ErrorMessage"] = "Dosya seçilmedi.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        content.Add(new StringContent(isMain.ToString()), "isMain");

        var result = await _apiService.PostMultipartAsync<string>($"api/products/{id}/images", content);

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = "Resim eklendi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var result = await _apiService.DeleteAsync<string>($"api/products/{id}/images/{imageId}");

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = "Resim silindi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMainImage(Guid id, Guid imageId)
    {
        var result = await _apiService.PatchAsync<string>($"api/products/{id}/images/{imageId}/set-main");

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = "Ana resim güncellendi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiService.DeleteAsync<string>($"api/products/{id}");
        if (result.IsSuccessful) TempData["SuccessMessage"] = result.Data;
        else TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault();

        return RedirectToAction(nameof(Index));
    }

    // --- HELPERS ---

    private async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var result = await _apiService.GetAsync<List<CategoryDto>>("api/categories");
        return result.IsSuccessful && result.Data != null
            ? result.Data.OrderBy(c => c.Name).ToList()
            : new List<CategoryDto>();
    }

    private List<SelectListItem> GetCurrencySelectList()
    {
        return new List<SelectListItem> {
            new() { Text = "TRY - Türk Lirası", Value = "TRY" },
            new() { Text = "USD - Amerikan Doları", Value = "USD" },
            new() { Text = "EUR - Euro", Value = "EUR" }
        };
    }

    private List<Guid> GetCategoryHierarchy(List<CategoryDto> categories, Guid categoryId)
    {
        var hierarchy = new List<Guid>();
        var current = categories.FirstOrDefault(c => c.Id == categoryId);

        while (current != null)
        {
            hierarchy.Insert(0, current.Id);
            current = current.ParentId.HasValue
                ? categories.FirstOrDefault(c => c.Id == current.ParentId.Value)
                : null;
        }

        return hierarchy;
    }
}