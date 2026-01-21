using ECommercePlatform.MvcAdmin.DTOs.Company;
using ECommercePlatform.MvcAdmin.Models.Company;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class CompanyController : Controller
{
    private readonly IApiService _apiService;

    public CompanyController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: /Company
    [HttpGet]
    public async Task<IActionResult> Index(string tab = "general")
    {
        var viewModel = new CompanySettingsViewModel
        {
            ActiveTab = tab
        };

        // 1. Şirket bilgilerini al
        var companyResult = await _apiService.GetAsync<CompanyDto>("api/companies/me");
        if (companyResult.IsSuccessful && companyResult.Data != null)
        {
            viewModel.Company = companyResult.Data;
        }
        else
        {
            TempData["ErrorMessage"] = companyResult.ErrorMessages?.FirstOrDefault() ?? "Şirket bilgileri yüklenemedi.";
        }

        // 2. Kargo ayarlarını al
        var shippingResult = await _apiService.GetAsync<ShippingSettingsDto>("api/companies/shipping-settings");
        if (shippingResult.IsSuccessful && shippingResult.Data != null)
        {
            viewModel.ShippingSettings = shippingResult.Data;
        }

        return View(viewModel);
    }

    // POST: /Company/Update (Şirket Bilgileri)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateCompanyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Lütfen formu doğru şekilde doldurunuz.";
            return RedirectToAction(nameof(Index), new { tab = "general" });
        }

        var command = new
        {
            Name = model.Name,
            TaxNumber = model.TaxNumber,
            City = model.City,
            District = model.District ?? "",
            Street = model.Street ?? "",
            ZipCode = model.ZipCode ?? "",
            FullAddress = model.FullAddress ?? ""
        };

        var result = await _apiService.PutAsync<string>("api/companies/me", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Şirket bilgileri güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Güncelleme başarısız.";
        }

        return RedirectToAction(nameof(Index), new { tab = "general" });
    }

    // POST: /Company/UpdateShipping (Kargo Ayarları)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShipping(UpdateShippingSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Lütfen geçerli değerler giriniz.";
            return RedirectToAction(nameof(Index), new { tab = "shipping" });
        }

        var command = new
        {
            FreeShippingThreshold = model.FreeShippingThreshold,
            FlatRate = model.FlatRate
        };

        var result = await _apiService.PutAsync<string>("api/companies/shipping-settings", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Kargo ayarları güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Güncelleme başarısız.";
        }

        return RedirectToAction(nameof(Index), new { tab = "shipping" });
    }

    // POST: /Company/Delete (Hesap Silme)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete()
    {
        var result = await _apiService.DeleteAsync<string>("api/companies/me");

        if (result.IsSuccessful)
        {
            // Session temizle ve login'e yönlendir
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = result.Data ?? "Şirket hesabı silindi.";
            return RedirectToAction("Login", "Auth");
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Hesap silinemedi.";
        return RedirectToAction(nameof(Index), new { tab = "danger" });
    }
}