using ECommercePlatform.Application.Auth.Login;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.Login;

public class TenantLoginCommandValidatorTests
{
    private readonly TenantLoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailOrUserNameIsEmpty()
    {
        var command = new TenantLoginCommand(string.Empty, "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EmailOrUserName);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PasswordIsEmpty()
    {
        var command = new TenantLoginCommand("test@test.com", string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new TenantLoginCommand("test@test.com", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
