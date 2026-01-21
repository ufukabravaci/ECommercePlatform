using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models.Brands;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class BrandController : Controller
{
    private readonly IApiService _apiService;

    public BrandController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // LIST
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var endpoint = $"api/brands?pageNumber={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        var result = await _apiService.GetAsync<PageResult<BrandDto>>(endpoint);

        var viewModel = new BrandListViewModel
        {
            SearchTerm = search
        };

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Brands = result.Data;
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Markalar yüklenirken bir hata oluştu.";
            viewModel.Brands = new PageResult<BrandDto>();
        }

        return View(viewModel);
    }

    // CREATE (GET)
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBrandViewModel());
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBrandViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new
        {
            Name = model.Name,
            LogoUrl = model.LogoUrl
        };

        var result = await _apiService.PostAsync<Guid>("api/brands", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = "Marka başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Marka oluşturulamadı.";
        return View(model);
    }

    // EDIT (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        // Tüm markaları çek ve içinden bul (API'de GetById yok)
        var result = await _apiService.GetAsync<PageResult<BrandDto>>($"api/brands?pageSize=1000");

        if (!result.IsSuccessful || result.Data == null)
        {
            TempData["ErrorMessage"] = "Marka bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var brand = result.Data.Items.FirstOrDefault(b => b.Id == id);
        if (brand == null)
        {
            TempData["ErrorMessage"] = "Marka bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateBrandViewModel
        {
            Id = brand.Id,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl
        };

        return View(model);
    }

    // EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBrandViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new
        {
            Id = model.Id,
            Name = model.Name,
            LogoUrl = model.LogoUrl
        };

        var result = await _apiService.PutAsync<string>("api/brands", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Marka başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Marka güncellenemedi.";
        return View(model);
    }

    // DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz marka ID'si.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.DeleteAsync<string>($"api/brands/{id}");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Marka başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Marka silinemedi.";
        }

        return RedirectToAction(nameof(Index));
    }
}