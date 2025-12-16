using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models;
using ECommercePlatform.MvcAdmin.Services;
using Mapster;
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

    #region LOGIN
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // API'deki "LoginCommand" property ismine uyum sağlama (EmailOrUserName)
        var apiRequest = new { model.EmailOrUserName, model.Password };

        var result = await _apiService.PostAsync<LoginResponseDto>("api/auth/login", apiRequest);

        if (!result.IsSuccessful || result.Data is null)
        {
            ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Giriş başarısız.";
            return View(model);
        }

        var response = result.Data;

        // A. 2FA Gerekli mi?
        if (response.RequiresTwoFactor)
        {
            return RedirectToAction("TwoFactorLogin", new { email = model.EmailOrUserName });
        }

        // B. Email Onayı Gerekli mi? (API destekliyorsa)
        if (response.RequiresEmailConfirmation)
        {
            ViewBag.Error = "Lütfen email adresinizi onaylayın.";
            return View(model);
        }

        // C. Normal Giriş (Token Set Etme)
        if (!string.IsNullOrEmpty(response.AccessToken))
        {
            SetSession(response.AccessToken);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Beklenmeyen bir hata oluştu.";
        return View(model);
    }
    #endregion

    #region 2FA LOGIN
    [HttpGet]
    public IActionResult TwoFactorLogin(string email)
    {
        return View(new TwoFactorLoginViewModel { Email = email });
    }

    [HttpPost]
    public async Task<IActionResult> TwoFactorLogin(TwoFactorLoginViewModel model)
    {
        // API Endpoint: /api/auth/login-2fa
        // API Beklentisi: { Email, Code }
        var result = await _apiService.PostAsync<LoginResponseDto>("api/auth/login-2fa", model);

        if (!result.IsSuccessful || result.Data is null)
        {
            ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Kod hatalı veya süresi dolmuş.";
            return View(model);
        }

        SetSession(result.Data.AccessToken!);
        return RedirectToAction("Index", "Home");
    }
    #endregion

    #region REGISTER (COMPANY)
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterCompanyViewModel model)
    {
        var requestDto = model.Adapt<RegisterCompanyRequestDto>(); //Mapster

        var result = await _apiService.PostAsync<string>("api/auth/register-tenant", requestDto);

        if (result.IsSuccessful)
        {
            // Kayıt başarılı, login sayfasına yönlendir
            TempData["SuccessMessage"] = result.Data; // "Kayıt başarılı..." mesajı
            return RedirectToAction("ConfirmEmail", new { email = model.Email });
        }

        ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Kayıt başarısız.";
        return View(model);
    }
    [HttpGet]
    public IActionResult ConfirmEmail(string email)
    {
        return View(new ConfirmEmailViewModel { Email = email });
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
    {
        var command = new { Email = model.Email, Token = model.Code };

        var result = await _apiService.PostAsync<string>("api/auth/confirm-email", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = "Email doğrulandı! Lütfen giriş yapınız.";
            // Login sayfasına email bilgisini taşıma şansımız yok (ViewModel farklı)
            // Ama kullanıcı deneyimi için çok da dert değil.
            return RedirectToAction("Login");
        }

        ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Kod hatalı.";
        return View(model);
    }
    #endregion

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // Ortak Token İşleme Metodu
    private void SetSession(string token)
    {
        HttpContext.Session.SetString("AccessToken", token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var companyId = jwtToken.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
        var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name")?.Value;

        if (userName != null) HttpContext.Session.SetString("UserName", userName);
        if (companyId != null) HttpContext.Session.SetString("CompanyId", companyId);
    }
}