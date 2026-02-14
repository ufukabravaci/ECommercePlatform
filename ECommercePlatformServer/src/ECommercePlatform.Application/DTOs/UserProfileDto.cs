public sealed record UserProfileDto(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    string? PhoneNumber,
    AddressDto? Address // ValueObject'i DTO olarak dönüyoruz
);

public sealed record AddressDto(
    string City,
    string District,
    string Street,
    string ZipCode,
    string FullAddress
);

// Application/DTOs/UpdateProfileDto.cs
// email ve username güncellenemez, sadece profil bilgileri güncellenebilir.
// onlar için ayrı bir DTO oluşturulabilir.
public sealed record UpdateProfileDto(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    AddressDto? Address
);