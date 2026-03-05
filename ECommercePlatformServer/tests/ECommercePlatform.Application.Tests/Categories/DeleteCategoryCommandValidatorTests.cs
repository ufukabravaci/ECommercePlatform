using ECommercePlatform.Application.Categories;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Categories;

public class DeleteCategoryCommandValidatorTests
{
    private readonly DeleteCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_IdIsEmpty()
    {
        var command = new DeleteCategoryCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_IdIsValid()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
