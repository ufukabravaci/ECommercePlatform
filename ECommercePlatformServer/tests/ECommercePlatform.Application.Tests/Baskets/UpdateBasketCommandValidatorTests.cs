using ECommercePlatform.Application.Baskets;
using ECommercePlatform.Domain.Baskets;
using FluentValidation.TestHelper;

namespace ECommercePlatform.Application.Tests.Baskets;

public class UpdateBasketCommandValidatorTests
{
    private readonly UpdateBasketCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_ProductIdIsEmpty()
    {
        var command = new UpdateBasketCommand(new List<BasketItem>
        {
            new BasketItem { ProductId = Guid.Empty, Quantity = 1, PriceAmount = 100 }
        });

        var result = _validator.TestValidate(command);

        // ChildRules için doğrulama yaparken property path'ini belirtmemiz gerekir
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_QuantityIsZeroOrLess()
    {
        var command = new UpdateBasketCommand(new List<BasketItem>
        {
            new BasketItem { ProductId = Guid.NewGuid(), Quantity = 0, PriceAmount = 100 }
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Validate_ShouldHaveError_When_PriceIsNegative()
    {
        var command = new UpdateBasketCommand(new List<BasketItem>
        {
            new BasketItem { ProductId = Guid.NewGuid(), Quantity = 1, PriceAmount = -5 }
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].PriceAmount");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = new UpdateBasketCommand(new List<BasketItem>
        {
            new BasketItem { ProductId = Guid.NewGuid(), Quantity = 2, PriceAmount = 150.5m, ProductName = "Test", PriceCurrency = "USD" }
        });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
