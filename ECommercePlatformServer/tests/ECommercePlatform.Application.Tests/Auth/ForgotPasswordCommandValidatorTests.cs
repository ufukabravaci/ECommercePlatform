using ECommercePlatform.Application.Auth;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsEmpty()
    {
        var command = new ForgotPasswordCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("E-mail adresi boş olamaz.");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = new ForgotPasswordCommand("invalid-email-format");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Geçersiz e-mail adresi.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_EmailIsValid()
    {
        var command = new ForgotPasswordCommand("test@test.com");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
