using ECommercePlatform.MvcAdmin.DTOs.Dashboard;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;

    public HomeController(IApiService apiService, IConfiguration configuration)
    {
        _apiService = apiService;
        _configuration = configuration;
    }
    private string GetApiBaseUrl() // html tarafında kullanmak için
    {
        return _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
    }
    public async Task<IActionResult> Index()
    {
        // 1. ESKİ ÇALIŞMA ŞEKLİNİ KORU: Login Kontrolü
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        // 2. VIEW DATA HAZIRLIĞI
        ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
        ViewBag.CompanyId = HttpContext.Session.GetString("CompanyId") ?? "Yok (Admin)";
        ViewBag.ApiBaseUrl = GetApiBaseUrl();

        // 3. DASHBOARD VERİSİ
        var result = await _apiService.GetAsync<DashboardStatsDto>("api/dashboard/stats");

        if (result.IsSuccessful && result.Data != null)
        {
            return View(result.Data);
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Veriler yüklenemedi.";
        return View(new DashboardStatsDto());
    }
}
