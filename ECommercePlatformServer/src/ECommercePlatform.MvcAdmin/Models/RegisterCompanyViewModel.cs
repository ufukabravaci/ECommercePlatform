namespace ECommercePlatform.MvcAdmin.Models;

public record RegisterCompanyViewModel(
    // User
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    // Company
    string CompanyName,
    string TaxNumber,
    string TaxOffice,
    string City,
    string District,
    string Street,
    string ZipCode,
    string FullAddress
);
