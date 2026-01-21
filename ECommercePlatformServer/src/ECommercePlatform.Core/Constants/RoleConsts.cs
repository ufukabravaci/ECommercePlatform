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
    //Şimdilik gerek yok
    //[Description("Yönetici")]
    //public const string Manager = "Manager";

    //[Description("Muhasebeci")]
    //public const string Accountant = "Accountant";

    //[Description("Müşteri Destek")]
    //public const string Support = "Support";

    //[Description("Mağaza Yöneticisi")]
    //public const string StoreManager = "StoreManager";

    //[Description("Depo Sorumlusu")]
    //public const string WarehouseStaff = "WarehouseStaff";
}
