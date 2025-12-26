public sealed record CompanyDto(
    Guid Id,
    string Name,
    string TaxNumber,
    string City,
    string District,
    string Street,
    string FullAddress,
    DateTimeOffset CreatedAt
);