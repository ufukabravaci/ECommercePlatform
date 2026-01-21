namespace ECommercePlatform.MvcAdmin.DTOs.Company;

public record ShippingSettingsDto(
    decimal FreeShippingThreshold,
    decimal FlatRate
);
