using ECommercePlatform.MvcAdmin.DTOs.Company;

namespace ECommercePlatform.MvcAdmin.Models.Company;

public class CompanySettingsViewModel
{
    public CompanyDto? Company { get; set; }
    public ShippingSettingsDto? ShippingSettings { get; set; }

    // Aktif tab
    public string ActiveTab { get; set; } = "general";
}
