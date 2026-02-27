using ECommercePlatform.Application.Auth.RefreshToken;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_RefreshTokenIsEmpty()
    {
        var command = new RefreshTokenCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
              .WithErrorMessage("RefreshToken alanı boş olamaz.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new RefreshTokenCommand("valid-token-123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
