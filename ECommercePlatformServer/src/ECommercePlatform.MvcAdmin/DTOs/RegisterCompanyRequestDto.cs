namespace ECommercePlatform.MvcAdmin.DTOs;

public record RegisterCompanyRequestDto(
    string FirstName, string LastName, string Email, string Password, string ConfirmPassword,
    string CompanyName, string TaxNumber, string TaxOffice, string FullAddress,
    string City, string District, string Street, string ZipCode
);
