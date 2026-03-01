using ECommercePlatform.Application.Brands;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Brands;

public class CreateBrandCommandValidatorTests
{
    private readonly CreateBrandCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_NameIsEmpty()
    {
        var command = new CreateBrandCommand(string.Empty, "logo.png");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_NameIsTooShort()
    {
        var command = new CreateBrandCommand("A", "logo.png");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new CreateBrandCommand("Apple", "logo.png");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
