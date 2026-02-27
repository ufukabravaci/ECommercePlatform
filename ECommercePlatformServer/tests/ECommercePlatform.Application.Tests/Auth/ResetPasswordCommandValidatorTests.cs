using ECommercePlatform.Application.Auth;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = ValidCommand() with { Email = "invalid-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_TokenIsEmpty()
    {
        var command = ValidCommand() with { Token = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_NewPasswordIsTooShort()
    {
        var command = ValidCommand() with { NewPassword = "123", ConfirmNewPassword = "123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Şifre alanı en az 6 karakterden oluşmalıdır.");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PasswordsDoNotMatch()
    {
        var command = ValidCommand() with { NewPassword = "Password123!", ConfirmNewPassword = "DifferentPassword123!" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword)
              .WithErrorMessage("Şifreler eşleşmiyor.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = ValidCommand();
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static ResetPasswordCommand ValidCommand() => new(
        Email: "test@test.com",
        Token: "valid-token-123",
        NewPassword: "Password123!",
        ConfirmNewPassword: "Password123!"
    );
}
