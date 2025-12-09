namespace ECommercePlatform.Domain.Users.ValueObjects;

public sealed record Address
{
    public Address(string city, string district, string street, string zipCode, string fullAddress)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("Şehir boş olamaz.");
        if (string.IsNullOrWhiteSpace(district)) throw new ArgumentException("İlçe boş olamaz.");
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Sokak/Cadde boş olamaz.");

        City = city;
        District = district;
        Street = street;
        ZipCode = zipCode ?? string.Empty;
        FullAddress = fullAddress ?? string.Empty;
    }

    public string City { get; init; }
    public string District { get; init; }
    public string Street { get; init; }
    public string ZipCode { get; init; }
    public string FullAddress { get; init; }
}