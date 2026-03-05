using ECommercePlatform.Application.Categories;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Categories;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_IdIsEmpty()
    {
        var command = new UpdateCategoryCommand(Guid.Empty, "Name", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_ParentIdIsEqualToId()
    {
        // Category Id ile Parent Id aynı verilmiş! Döngüsel hata yaratır.
        var id = Guid.NewGuid();
        var command = new UpdateCategoryCommand(id, "Test", id);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ParentId)
              .WithErrorMessage("Kategori kendisinin üst kategorisi olamaz.");
    }
}
