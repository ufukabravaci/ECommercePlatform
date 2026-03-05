using ECommercePlatform.Application.Companies;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Companies;

public class UpdateCompanyCommandValidatorTests
{
    private readonly UpdateCompanyCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_NameIsTooShort()
    {
        var command = new UpdateCompanyCommand("A", "1234567890", "City", "Dist", "Street", "Zip", "Full");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_TaxNumberIsInvalidLength()
    {
        var command = new UpdateCompanyCommand("Test Company", "123", "City", "Dist", "Street", "Zip", "Full");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaxNumber);
    }
}
