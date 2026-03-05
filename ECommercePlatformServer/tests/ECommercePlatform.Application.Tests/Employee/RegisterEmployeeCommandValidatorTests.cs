using ECommercePlatform.Application.Employee;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Employee;

public class RegisterEmployeeCommandValidatorTests
{
    private readonly RegisterEmployeeCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_TokenIsEmpty()
    {
        var command = new RegisterEmployeeCommand("", "A", "B", "123456", "123456");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PasswordsDoNotMatch()
    {
        var command = new RegisterEmployeeCommand("token", "Ad", "Soyad", "123456", "654321");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
