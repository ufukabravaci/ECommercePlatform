using ECommercePlatform.MvcAdmin.DTOs;
using ECommercePlatform.MvcAdmin.DTOs.Orders;
using ECommercePlatform.MvcAdmin.Models.Orders;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class OrderController : Controller
{
    private readonly IApiService _apiService;

    public OrderController(IApiService apiService)
    {
        _apiService = apiService;
    }

    // LIST - Tüm Siparişler
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var endpoint = $"api/orders?pageNumber={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        var result = await _apiService.GetAsync<PageResult<OrderListDto>>(endpoint);

        var viewModel = new OrderListViewModel
        {
            SearchTerm = search,
            StatusFilter = status
        };

        if (result.IsSuccessful && result.Data != null)
        {
            viewModel.Orders = result.Data;
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Siparişler yüklenirken bir hata oluştu.";
            viewModel.Orders = new PageResult<OrderListDto>();
        }

        return View(viewModel);
    }

    // DETAIL - Sipariş Detayı
    [HttpGet]
    public async Task<IActionResult> Detail(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            TempData["ErrorMessage"] = "Sipariş numarası belirtilmedi.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.GetAsync<OrderDetailDto>($"api/orders/{orderNumber}");

        if (!result.IsSuccessful || result.Data == null)
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Sipariş bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var currentStatusValue = OrderStatusHelper.GetStatusValue(result.Data.Status);

        var viewModel = new OrderDetailViewModel
        {
            Order = result.Data,
            AvailableStatuses = GetAvailableStatuses(currentStatusValue)
        };

        return View(viewModel);
    }

    // UPDATE STATUS - Durum Güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string orderNumber, int status)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            TempData["ErrorMessage"] = "Sipariş numarası belirtilmedi.";
            return RedirectToAction(nameof(Index));
        }

        // API integer bekliyor: [FromBody] int status
        var result = await _apiService.PatchAsync<string>($"api/orders/{orderNumber}/status", status);

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Sipariş durumu güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Durum güncellenemedi.";
        }

        return RedirectToAction(nameof(Detail), new { orderNumber });
    }

    // ADD TRACKING NUMBER - Kargo Takip Numarası Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTracking(UpdateTrackingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Geçersiz takip numarası.";
            return RedirectToAction(nameof(Detail), new { orderNumber = model.OrderNumber });
        }

        // API [FromBody] string trackingNumber bekliyor
        var result = await _apiService.PatchAsync<string>(
            $"api/orders/{model.OrderNumber}/tracking",
            model.TrackingNumber
        );

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Kargo takip numarası eklendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Takip numarası eklenemedi.";
        }

        return RedirectToAction(nameof(Detail), new { orderNumber = model.OrderNumber });
    }

    // REFUND - İade İşlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            TempData["ErrorMessage"] = "Sipariş numarası belirtilmedi.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.PatchAsync<string>($"api/orders/{orderNumber}/refund");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "İade işlemi başarılı.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "İade işlemi başarısız.";
        }

        return RedirectToAction(nameof(Detail), new { orderNumber });
    }

    // DELETE - Sipariş Silme/İptal
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            TempData["ErrorMessage"] = "Sipariş numarası belirtilmedi.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _apiService.DeleteAsync<string>($"api/orders/{orderNumber}");

        if (result.IsSuccessful)
        {
            TempData["SuccessMessage"] = result.Data ?? "Sipariş iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessages?.FirstOrDefault() ?? "Sipariş iptal edilemedi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // HELPER: Mevcut duruma göre geçiş yapılabilecek durumları getir
    private static List<OrderStatusOption> GetAvailableStatuses(int currentStatus)
    {
        var allStatuses = OrderStatusHelper.GetAllStatuses();
        var result = new List<OrderStatusOption>();

        foreach (var (value, text) in allStatuses)
        {
            result.Add(new OrderStatusOption
            {
                Value = value,
                Text = text,
                IsSelected = value == currentStatus,
                IsDisabled = !CanTransitionTo(currentStatus, value)
            });
        }

        return result;
    }

    // HELPER: Durum geçişi kontrolü
    private static bool CanTransitionTo(int currentStatus, int targetStatus)
    {
        if (currentStatus == targetStatus) return false;

        // İptal veya iade edilmiş siparişler değiştirilemez
        if (currentStatus is 5 or 6) return false;

        // Teslim edilmiş siparişler sadece iade edilebilir
        if (currentStatus == 4) return targetStatus == 6;

        return currentStatus switch
        {
            0 => targetStatus is 1 or 5, // Pending -> Confirmed, Cancelled
            1 => targetStatus is 2 or 5, // Confirmed -> Processing, Cancelled
            2 => targetStatus is 3 or 5, // Processing -> Shipped, Cancelled
            3 => targetStatus is 4 or 5, // Shipped -> Delivered, Cancelled
            _ => false
        };
    }
}