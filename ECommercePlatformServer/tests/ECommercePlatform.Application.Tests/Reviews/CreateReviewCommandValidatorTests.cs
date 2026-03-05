using ECommercePlatform.Application.Reviews;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Reviews;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_ShouldHaveError_When_RatingIsOutOfRange(int invalidRating)
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), invalidRating, "Güzel");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_CommentIsEmpty()
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }
}
