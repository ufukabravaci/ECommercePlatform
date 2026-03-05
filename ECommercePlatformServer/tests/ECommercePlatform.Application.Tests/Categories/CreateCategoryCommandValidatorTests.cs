using ECommercePlatform.Application.Categories;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Categories;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_NameIsTooShort()
    {
        var command = new CreateCategoryCommand("A", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_ParentIdIsNotEmptyButInvalid()
    {
        // Guid.Empty olamaz
        var command = new CreateCategoryCommand("Kategori", Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ParentId)
              .WithErrorMessage("Geçersiz üst kategori bilgisi.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_ParentIdIsNull()
    {
        var command = new CreateCategoryCommand("Elektronik", null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
