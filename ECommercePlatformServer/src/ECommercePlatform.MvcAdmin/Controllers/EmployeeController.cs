using ECommercePlatform.MvcAdmin.DTOs.Employee;
using ECommercePlatform.MvcAdmin.Models.Employee;
using ECommercePlatform.MvcAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class EmployeeController : Controller
{
    private readonly IApiService _apiService;

    public EmployeeController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new EmployeeListViewModel();

        // 1. Çalışan listesi
        var employeesResult = await _apiService.GetAsync<List<EmployeeDto>>("api/employees");
        if (employeesResult.IsSuccessful && employeesResult.Data != null)
        {
            viewModel.Employees = employeesResult.Data;
        }
        else
        {
            TempData["ErrorMessage"] = employeesResult.ErrorMessages?.FirstOrDefault() ?? "Çalışanlar yüklenirken hata oluştu.";
        }

        // 2. Permission listesi
        var permissionsResult = await _apiService.GetAsync<List<PermissionGroupDto>>("api/employees/permissions-list");
        if (permissionsResult.IsSuccessful && permissionsResult.Data != null)
        {
            viewModel.PermissionGroups = permissionsResult.Data;
        }

        // 3. Atanabilir roller
        var rolesResult = await _apiService.GetAsync<List<RoleDto>>("api/employees/roles");
        if (rolesResult.IsSuccessful && rolesResult.Data != null)
        {
            viewModel.AvailableRoles = rolesResult.Data;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite([FromBody] InviteEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        var result = await _apiService.PostAsync<string>("api/employees/invite", new
        {
            Email = model.Email,
            Role = model.Role
        });

        return Json(result.IsSuccessful
            ? new { success = true, message = result.Data ?? "Davet gönderildi." }
            : new { success = false, message = result.ErrorMessages?.FirstOrDefault() ?? "Hata oluştu." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions([FromBody] UpdatePermissionsBatchViewModel model)
    {
        if (model.UserId == Guid.Empty)
            return Json(new { success = false, message = "Geçersiz kullanıcı." });

        var errors = new List<string>();
        var successCount = 0;

        foreach (var change in model.Changes)
        {
            var result = await _apiService.PutAsync<string>("api/employees/permissions", new
            {
                UserId = model.UserId,
                Permission = change.Permission,
                IsGranted = change.IsGranted
            });

            if (result.IsSuccessful) successCount++;
            else errors.Add($"{change.Permission}: {result.ErrorMessages?.FirstOrDefault()}");
        }

        return Json(errors.Any()
            ? new { success = false, message = $"{successCount} güncellendi, {errors.Count} hata.", errors }
            : new { success = true, message = $"{successCount} yetki güncellendi." });
    }
}