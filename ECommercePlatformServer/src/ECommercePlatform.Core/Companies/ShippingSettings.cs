namespace ECommercePlatform.Domain.Companies;

public sealed record ShippingSettings
{
    public decimal FreeShippingThreshold { get; init; } // Kaç TL üzeri bedava?
    public decimal FlatRate { get; init; } // Sabit ücret

    public ShippingSettings(decimal freeShippingThreshold, decimal flatRate)
    {
        if (freeShippingThreshold < 0) throw new ArgumentException("Limit 0'dan küçük olamaz.");
        if (flatRate < 0) throw new ArgumentException("Kargo ücreti 0'dan küçük olamaz.");

        FreeShippingThreshold = freeShippingThreshold;
        FlatRate = flatRate;
    }

    public static ShippingSettings Default => new(0, 0);
}
