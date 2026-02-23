using System.ComponentModel;

namespace ECommercePlatform.Domain.Constants;

public static class RoleConsts
{
    [Description("Süper Yönetici")]
    public const string SuperAdmin = "SuperAdmin";

    [Description("Şirket Sahibi")]
    public const string CompanyOwner = "CompanyOwner";

    [Description("Müşteri")]
    public const string Customer = "Customer";

    // --- Atanabilir Çalışan Rolleri ---
    [Description("Çalışan")]
    public const string Employee = "Employee";
}
