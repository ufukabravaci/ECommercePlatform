using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models.Banners;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class BannerController : Controller
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;

    public BannerController(IApiService apiService, IConfiguration configuration)
    {
        _apiService = apiService;
        _configuration = configuration;
    }

    private string GetApiBaseUrl()
    {
        return _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
    }

    // LIST
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _apiService.GetAsync<List<BannerDto>>("api/banners");

        var viewModel = new BannerListViewModel();

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Banners = result.Data.OrderBy(b => b.Order).ToList();
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Bannerlar yüklenirken bir hata oluştu.";
        }

        ViewBag.ApiBaseUrl = GetApiBaseUrl();
        return View(viewModel);
    }

    // CREATE (GET)
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateBannerViewModel
        {
            Order = 1
        };
        return View(model);
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBannerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Dosya validasyonu
        if (model.Image != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(model.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Image", "Sadece JPG, PNG, GIF ve WebP formatları desteklenir.");
                return View(model);
            }

            if (model.Image.Length > 5 * 1024 * 1024) // 5MB
            {
                ModelState.AddModelError("Image", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                return View(model);
            }
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Title), "Title");
        content.Add(new StringContent(model.Description), "Description");
        content.Add(new StringContent(model.TargetUrl), "TargetUrl");
        content.Add(new StringContent(model.Order.ToString()), "Order");

        if (model.Image != null)
        {
            var streamContent = new StreamContent(model.Image.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.Image.ContentType);
            content.Add(streamContent, "Image", model.Image.FileName);
        }

        var result = await _apiService.PostMultipartAsync<Guid>("api/banners", content);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = "Banner başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Banner oluşturulamadı.";
        return View(model);
    }

    // EDIT (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiService.GetAsync<List<BannerDto>>("api/banners");

        if (!result.IsSuccessful || result.Data == null)
        {
            TempData["ErrorMessage"] = "Banner bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var banner = result.Data.FirstOrDefault(b => b.Id == id);
        if (banner == null)
        {
            TempData["ErrorMessage"] = "Banner bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateBannerViewModel
        {
            Id = banner.Id,
            Title = banner.Title,
            Description = banner.Description,
            TargetUrl = banner.TargetUrl,
            Order = banner.Order,
            CurrentImageUrl = banner.ImageUrl
        };

        ViewBag.ApiBaseUrl = GetApiBaseUrl();
        return View(model);
    }

    // EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBannerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            return View(model);
        }

        // Dosya validasyonu (opsiyonel dosya)
        if (model.Image != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(model.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Image", "Sadece JPG, PNG, GIF ve WebP formatları desteklenir.");
                ViewBag.ApiBaseUrl = GetApiBaseUrl();
                return View(model);
            }

            if (model.Image.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Image", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                ViewBag.ApiBaseUrl = GetApiBaseUrl();
                return View(model);
            }
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Id.ToString()), "Id");
        content.Add(new StringContent(model.Title), "Title");
        content.Add(new StringContent(model.Description), "Description");
        content.Add(new StringContent(model.TargetUrl), "TargetUrl");
        content.Add(new StringContent(model.Order.ToString()), "Order");

        if (model.Image != null)
        {
            var streamContent = new StreamContent(model.Image.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.Image.ContentType);
            content.Add(streamContent, "Image", model.Image.FileName);
        }

        var result = await _apiService.PutMultipartAsync<string>("api/banners", content);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Banner başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Banner güncellenemedi.";
        ViewBag.ApiBaseUrl = GetApiBaseUrl();
        return View(model);
    }

    // DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz banner ID'si.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.DeleteAsync<string>($"api/banners/{id}");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Banner başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Banner silinemedi.";
        }

        return RedirectToAction(nameof(Index));
    }
}