using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class AuthController : Controller
{
    private readonly IApiService _apiService;

    public AuthController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("AccessToken") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _apiService.PostAsync<LoginResponseDto>("api/auth/login", model);

        if (!result.IsSuccessful)
        {
            ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Giriş başarısız.";
            return View(model);
        }

        if (result.Data is null)
        {
            ViewBag.Error = "Sunucudan boş cevap döndü.";
            return View(model);
        }

        var response = result.Data;

        // A. Email Onayı
        if (response.RequiresEmailConfirmation)
        {
            return RedirectToAction("ConfirmEmail", new { email = model.EmailOrUserName });
        }

        // B. 2FA
        if (response.RequiresTwoFactor)
        {
            return RedirectToAction("TwoFactorLogin", new { email = model.EmailOrUserName });
        }

        // C. Token
        if (!string.IsNullOrEmpty(response.AccessToken))
        {
            var token = response.AccessToken;
            HttpContext.Session.SetString("AccessToken", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var companyId = jwtToken.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name")?.Value;

            if (userName != null) HttpContext.Session.SetString("UserName", userName);
            if (companyId != null) HttpContext.Session.SetString("CompanyId", companyId);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Beklenmeyen bir durum oluştu.";
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
