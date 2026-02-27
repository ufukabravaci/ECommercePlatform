using ECommercePlatform.Application.Auth.Login;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Auth.Login;

public class TenantLoginWithTwoFactorCommandValidatorTests
{
    private readonly TenantLoginWithTwoFactorCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsEmpty()
    {
        var command = new TenantLoginWithTwoFactorCommand(string.Empty, "123456", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_EmailIsInvalid()
    {
        var command = new TenantLoginWithTwoFactorCommand("invalid-email", "123456", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_CodeIsEmpty()
    {
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", string.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_CompanyIdIsEmpty()
    {
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", "123456", Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CompanyId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", "123456", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
