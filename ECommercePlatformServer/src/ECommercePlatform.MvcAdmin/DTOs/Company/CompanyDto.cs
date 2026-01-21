namespace ECommercePlatform.MvcAdmin.DTOs.Company;

public record CompanyDto(
    Guid Id,
    string Name,
    string TaxNumber,
    string City,
    string District,
    string Street,
    string FullAddress,
    DateTimeOffset CreatedAt
);
