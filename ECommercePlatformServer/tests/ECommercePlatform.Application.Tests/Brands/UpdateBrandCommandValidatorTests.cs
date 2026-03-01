using ECommercePlatform.Application.Brands;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Brands;

public class UpdateBrandCommandValidatorTests
{
    private readonly UpdateBrandCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_IdIsEmpty()
    {
        var command = new UpdateBrandCommand(Guid.Empty, "Nike", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_NameIsInvalid()
    {
        var command = new UpdateBrandCommand(Guid.NewGuid(), "A", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
