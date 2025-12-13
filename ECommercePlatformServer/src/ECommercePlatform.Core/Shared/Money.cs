namespace ECommercePlatform.Domain.Shared;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Parasal miktar negatif olamaz.", nameof(amount));
        }
        Amount = amount;
        Currency = currency;
    }

    // Business Behaviors

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException("Farklı para birimleri doğrudan toplanamaz.");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException("Farklı para birimleri doğrudan çıkarılamaz.");
        }

        var newAmount = Amount - other.Amount;

        if (newAmount < 0)
        {
            throw new InvalidOperationException("Çıkarma işlemi bakiyenin sıfırdan küçük olmasına neden olamaz.");
        }

        return new Money(newAmount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public static Money Zero(Currency currency)
    {
        return new Money(0m, currency);
    }
}