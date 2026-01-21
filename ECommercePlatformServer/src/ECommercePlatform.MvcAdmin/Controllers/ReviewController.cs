using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.Models.Reviews;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class ReviewController : Controller
{
    private readonly IApiService _apiService;

    public ReviewController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // ==================== ALL - Tüm Yorumlar ====================
    [HttpGet]
    public async Task<IActionResult> All(
        int page = 1,
        int pageSize = 10,
        bool? isApproved = null,
        int? minRating = null,
        int? maxRating = null,
        string? searchTerm = null,
        string? sortBy = "CreatedAt",
        bool sortDescending = true)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={page}",
            $"pageSize={pageSize}",
            $"sortDescending={sortDescending}"
        };

        if (isApproved.HasValue)
            queryParams.Add($"isApproved={isApproved.Value}");
        if (minRating.HasValue)
            queryParams.Add($"minRating={minRating.Value}");
        if (maxRating.HasValue)
            queryParams.Add($"maxRating={maxRating.Value}");
        if (!string.IsNullOrWhiteSpace(searchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
        if (!string.IsNullOrWhiteSpace(sortBy))
            queryParams.Add($"sortBy={sortBy}");

        var endpoint = $"api/reviews?{string.Join("&", queryParams)}";
        var result = await _apiService.GetAsync<PageResult<ReviewDto>>(endpoint);

        var viewModel = new AllReviewsViewModel
        {
            IsApproved = isApproved,
            MinRating = minRating,
            MaxRating = maxRating,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Reviews = result.Data;

            // Sayfa içindeki verilerden istatistik hesapla
            if (result.Data.Items != null && result.Data.Items.Any())
            {
                viewModel.ApprovedCount = result.Data.Items.Count(x => x.IsApproved);
                viewModel.PendingCount = result.Data.Items.Count(x => !x.IsApproved);
                viewModel.RepliedCount = result.Data.Items.Count(x => !string.IsNullOrEmpty(x.SellerReply));
                viewModel.AverageRating = result.Data.Items.Average(x => x.Rating);
            }
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yorumlar yüklenirken bir hata oluştu.";
            viewModel.Reviews = new PageResult<ReviewDto>();
        }

        return View(viewModel);
    }

    // ==================== INDEX - Ürüne Ait Yorumlar ====================
    [HttpGet]
    public async Task<IActionResult> Index(Guid productId, string? productName = null, int page = 1, int pageSize = 10)
    {
        if (productId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz ürün ID'si.";
            return RedirectToAction("Index", "Product");
        }

        var endpoint = $"api/reviews/product/{productId}?pageNumber={page}&pageSize={pageSize}";
        var result = await _apiService.GetAsync<PageResult<ReviewDto>>(endpoint);

        var viewModel = new ReviewListViewModel
        {
            ProductId = productId,
            ProductName = productName
        };

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Reviews = result.Data;
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yorumlar yüklenirken bir hata oluştu.";
            viewModel.Reviews = new PageResult<ReviewDto>();
        }

        return View(viewModel);
    }

    // ==================== APPROVE - Yorumu Onayla ====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, Guid? productId = null, string? productName = null, string? returnUrl = null)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz yorum ID'si.";
            return RedirectToCorrectPage(productId, productName, returnUrl);
        }

        var result = await _apiService.PatchAsync<string>($"api/reviews/{id}/approve");

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = result.Data ?? "Yorum başarıyla onaylandı.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yorum onaylanamadı.";

        return RedirectToCorrectPage(productId, productName, returnUrl);
    }

    // ==================== REJECT - Yorumu Reddet ====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, Guid? productId = null, string? productName = null, string? returnUrl = null)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz yorum ID'si.";
            return RedirectToCorrectPage(productId, productName, returnUrl);
        }

        var result = await _apiService.PatchAsync<string>($"api/reviews/{id}/reject");

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = result.Data ?? "Yorum reddedildi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yorum reddedilemedi.";

        return RedirectToCorrectPage(productId, productName, returnUrl);
    }

    // ==================== DELETE - Yorumu Sil ====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid? productId = null, string? productName = null, string? returnUrl = null)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Geçersiz yorum ID'si.";
            return RedirectToCorrectPage(productId, productName, returnUrl);
        }

        var result = await _apiService.DeleteAsync<string>($"api/reviews/{id}");

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = result.Data ?? "Yorum başarıyla silindi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yorum silinemedi.";

        return RedirectToCorrectPage(productId, productName, returnUrl);
    }

    // ==================== REPLY - Yoruma Yanıt Ver ====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(ReplyReviewViewModel model, string? productName = null, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Geçersiz yanıt.";
            return RedirectToCorrectPage(model.ProductId, productName, returnUrl);
        }

        var result = await _apiService.PostAsync<string>($"api/reviews/{model.ReviewId}/reply", model.Reply);

        if (result.IsSuccessful)
            TempData["SuccessMessage"] = result.Data ?? "Yanıtınız başarıyla eklendi.";
        else
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Yanıt eklenemedi.";

        return RedirectToCorrectPage(model.ProductId, productName, returnUrl);
    }

    // ==================== HELPER ====================
    private IActionResult RedirectToCorrectPage(Guid? productId, string? productName, string? returnUrl)
    {
        // returnUrl varsa oraya git
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // ProductId varsa ürün yorumlarına git
        if (productId.HasValue && productId.Value != Guid.Empty)
            return RedirectToAction(nameof(Index), new { productId, productName });

        // Yoksa tüm yorumlara git
        return RedirectToAction(nameof(All));
    }
}