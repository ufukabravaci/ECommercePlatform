using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly IApiService _apiService;

    public CustomersController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, string search = "")
    {
        // API'ye Query Parametrelerini gönderiyoruz
        var queryString = $"?pageNumber={pageNumber}&pageSize=10&search={search}";

        var result = await _apiService.GetAsync<PageResult<CustomerDto>>($"api/customers{queryString}");

        if (!result.IsSuccessful || result.Data == null)
        {
            ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Veriler yüklenemedi.";
            return View(new CustomerListViewModel());
        }

        var model = new CustomerListViewModel
        {
            Customers = result.Data.Items,
            PageNumber = result.Data.PageNumber,
            TotalPages = result.Data.TotalPages,
            Search = search
        };

        return View(model);
    }
}