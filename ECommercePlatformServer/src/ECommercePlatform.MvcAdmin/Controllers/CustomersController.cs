using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class CustomerController : Controller
{
    private readonly IApiService _apiService;

    public CustomerController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // LIST - Tüm Müşteriler
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var endpoint = $"api/customers?pageNumber={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        var result = await _apiService.GetAsync<PageResult<CustomerDto>>(endpoint);

        var viewModel = new CustomerListViewModel
        {
            SearchTerm = search,
            StatusFilter = status
        };

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Customers = result.Data;

            // Client-side status filtering (API'de yoksa burada filtrele)
            if (!string.IsNullOrEmpty(status))
            {
                var filteredItems = status.ToLower() switch
                {
                    "active" => viewModel.Customers.Items.Where(c => c.IsActive).ToList(),
                    "inactive" => viewModel.Customers.Items.Where(c => !c.IsActive).ToList(),
                    _ => viewModel.Customers.Items
                };
                viewModel.Customers.Items = filteredItems;
            }
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Müşteriler yüklenirken bir hata oluştu.";
            viewModel.Customers = new PageResult<CustomerDto>();
        }

        return View(viewModel);
    }

    // DELETE - Müşteriyi Şirketten Çıkar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz müşteri ID'si.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.DeleteAsync<string>($"api/customers/{id}");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Müşteri başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Müşteri silinemedi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Müşterinin Siparişlerini Görüntüle (Order sayfasına yönlendir)
    [HttpGet]
    public IActionResult Orders(Guid id, string customerName)
    {
        // Order Index sayfasına müşteri filtresiyle yönlendir
        return RedirectToAction("Index", "Order", new { search = customerName });
    }
}