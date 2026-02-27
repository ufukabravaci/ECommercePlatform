using ECommercePlatform.Application.Auth.Register;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class RegisterCustomerCommandValidatorTests
{
    private readonly RegisterCustomerCommandValidator _validator = new();

    [Theory]
    [InlineData("Ali123")] // Rakam içeriyor
    [InlineData("Ali@")] // Özel karakter içeriyor
    public void Validate_ShouldHaveError_When_FirstNameContainsInvalidCharacters(string invalidName)
    {
        var command = ValidCommand() with { FirstName = invalidName };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Ad sadece harf ve boşluk içerebilir.");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = ValidCommand() with { Email = "invalid-email" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PasswordsDoNotMatch()
    {
        var command = ValidCommand() with { Password = "Password123!", ConfirmPassword = "Different123!" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("Şifreler uyuşmuyor.");
    }

    [Fact]
    public void Validate_ShouldNotHaveAnyError_When_CommandIsValid()
    {
        var command = ValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static RegisterCustomerCommand ValidCommand() => new(
        FirstName: "Ahmet",
        LastName: "Yılmaz",
        Email: "ahmet@test.com",
        Password: "Password123",
        ConfirmPassword: "Password123"
    );
}
