using ECommercePlatform.Application.Companies;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Companies;

public class UpdateShippingSettingsCommandValidatorTests
{
    private readonly UpdateShippingSettingsCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_ThresholdIsNegative()
    {
        var command = new UpdateShippingSettingsCommand(-10, 50);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FreeShippingThreshold);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_FlatRateIsNegative()
    {
        var command = new UpdateShippingSettingsCommand(100, -5);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FlatRate);
    }
}
