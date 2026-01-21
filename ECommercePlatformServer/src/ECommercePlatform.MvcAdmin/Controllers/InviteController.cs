using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models.Invite;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ECommercePlatform.MvcAdmin.Controllers;

[AllowAnonymous]
public class InviteController : Controller
{
    private readonly IApiService _apiService;

    public InviteController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: /Invite/Accept?token=xyz
    [HttpGet]
    public IActionResult Accept(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Geçersiz davet linki.";
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = new AcceptInviteViewModel
        {
            Token = token,
            ActiveTab = "register"
        };

        return View(viewModel);
    }

    // POST: /Invite/Register - Yeni Hesap Oluştur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AcceptInviteViewModel model)
    {
        // Validasyon
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(model.RegisterForm.FirstName))
            ModelState.AddModelError("RegisterForm.FirstName", "Ad zorunludur.");
        if (string.IsNullOrWhiteSpace(model.RegisterForm.LastName))
            ModelState.AddModelError("RegisterForm.LastName", "Soyad zorunludur.");
        if (string.IsNullOrWhiteSpace(model.RegisterForm.Password))
            ModelState.AddModelError("RegisterForm.Password", "Şifre zorunludur.");
        if (model.RegisterForm.Password?.Length < 6)
            ModelState.AddModelError("RegisterForm.Password", "Şifre en az 6 karakter olmalıdır.");
        if (model.RegisterForm.Password != model.RegisterForm.ConfirmPassword)
            ModelState.AddModelError("RegisterForm.ConfirmPassword", "Şifreler eşleşmiyor.");

        if (!ModelState.IsValid)
        {
            model.ActiveTab = "register";
            return View("Accept", model);
        }

        var command = new
        {
            Token = model.Token,
            FirstName = model.RegisterForm.FirstName,
            LastName = model.RegisterForm.LastName,
            Password = model.RegisterForm.Password,
            ConfirmPassword = model.RegisterForm.ConfirmPassword
        };

        var result = await _apiService.PostAsync<string>("api/employees/register", command);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.Error = result.ErrorMessages?.FirstOrDefault() ?? "Kayıt işlemi başarısız.";
        model.ActiveTab = "register";
        return View("Accept", model);
    }

    // POST: /Invite/LoginAndAccept - Mevcut Hesapla Giriş Yap ve Kabul Et
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAndAccept(AcceptInviteViewModel model)
    {
        // Validasyon
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(model.LoginForm.Email))
            ModelState.AddModelError("LoginForm.Email", "Email zorunludur.");
        if (string.IsNullOrWhiteSpace(model.LoginForm.Password))
            ModelState.AddModelError("LoginForm.Password", "Şifre zorunludur.");

        if (!ModelState.IsValid)
        {
            model.ActiveTab = "login";
            return View("Accept", model);
        }

        // 1. Login
        var loginRequest = new
        {
            EmailOrUserName = model.LoginForm.Email,
            Password = model.LoginForm.Password
        };

        var loginResult = await _apiService.PostAsync<LoginResponseDto>("api/auth/login", loginRequest);

        if (!loginResult.IsSuccessful || loginResult.Data?.AccessToken == null)
        {
            ViewBag.Error = loginResult.ErrorMessages?.FirstOrDefault() ?? "Giriş başarısız. Email veya şifre hatalı.";
            model.ActiveTab = "login";
            return View("Accept", model);
        }

        // 2. Token'ı Session'a kaydet
        HttpContext.Session.SetString("AccessToken", loginResult.Data.AccessToken);

        if (!string.IsNullOrEmpty(loginResult.Data.RefreshToken))
            HttpContext.Session.SetString("RefreshToken", loginResult.Data.RefreshToken);

        SetSessionFromToken(loginResult.Data.AccessToken);

        // 3. Daveti Kabul Et
        var acceptCommand = new { Token = model.Token };
        var acceptResult = await _apiService.PostAsync<string>("api/employees/accept-invite", acceptCommand);

        if (acceptResult.IsSuccessful)
        {
            TempData["SuccessMessage"] = acceptResult.Data ?? "Daveti kabul ettiniz. Şirkete katıldınız!";
            return RedirectToAction("Index", "Home");
        }

        // Başarısız - Session temizle
        HttpContext.Session.Clear();
        ViewBag.Error = acceptResult.ErrorMessages?.FirstOrDefault() ?? "Davet kabul edilemedi.";
        model.ActiveTab = "login";
        return View("Accept", model);
    }

    private void SetSessionFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var companyId = jwtToken.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
        var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name")?.Value;

        if (userName != null) HttpContext.Session.SetString("UserName", userName);
        if (companyId != null) HttpContext.Session.SetString("CompanyId", companyId);
    }
}
