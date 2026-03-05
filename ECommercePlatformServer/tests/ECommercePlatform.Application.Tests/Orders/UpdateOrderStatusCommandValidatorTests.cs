using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Orders;

public class UpdateOrderStatusCommandValidatorTests
{
    private readonly UpdateOrderStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_OrderNumberIsEmpty()
    {
        var command = new UpdateOrderStatusCommand("", OrderStatus.Confirmed);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderNumber);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_EnumIsInvalid()
    {
        // Olmayan bir enum değeri (Örn: 999) gönderiyoruz
        var command = new UpdateOrderStatusCommand("ORD-123", (OrderStatus)999);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewStatus);
    }
}
