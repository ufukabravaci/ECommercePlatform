using ECommercePlatform.Application.Auth.Register;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class ConfirmEmailCommandValidatorTests
{
    private readonly ConfirmEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = new ConfirmEmailCommand("invalid-email", "token-123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_TokenIsEmpty()
    {
        var command = new ConfirmEmailCommand("test@test.com", string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new ConfirmEmailCommand("test@test.com", "valid-token-123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
