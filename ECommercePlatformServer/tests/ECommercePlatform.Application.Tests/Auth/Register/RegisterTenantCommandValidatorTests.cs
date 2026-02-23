using ECommercePlatform.Application.Auth.Register;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class RegisterTenantCommandValidatorTests
{
    private readonly RegisterTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = ValidCommand() with { Email = "invalid-email-format" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Geçersiz e-mail adresi.");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PasswordsDoNotMatch()
    {
        var command = ValidCommand() with { Password = "Password123!", ConfirmPassword = "DifferentPassword!" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("Şifreler eşleşmiyor.");
    }

    [Theory]
    [InlineData("123456789")] // 9 hane (hata vermeli)
    [InlineData("123456789012")] // 12 hane (hata vermeli)
    public void Validate_ShouldHaveError_When_TaxNumberLengthIsInvalid(string invalidTaxNumber)
    {
        var command = ValidCommand() with { TaxNumber = invalidTaxNumber };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaxNumber)
              .WithErrorMessage("Vergi numarası 10-11 haneli olmalıdır.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = ValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static RegisterTenantCommand ValidCommand() => new(
        FirstName: "Ufuk",
        LastName: "Test",
        Email: "ufuk@test.com",
        Password: "Password123*",
        ConfirmPassword: "Password123*",
        CompanyName: "Ufuk Yazılım",
        TaxNumber: "1234567890",
        TaxOffice: "Besiktas",
        FullAddress: "Barbaros",
        City: "Istanbul",
        District: "Besiktas",
        Street: "Barbaros",
        ZipCode: "34353"
    );
}
