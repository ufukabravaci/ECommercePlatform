using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Users; // User referansı için (Collection)
using ECommercePlatform.Domain.Users.ValueObjects; // Address Value Object

namespace ECommercePlatform.Domain.Companies;

public sealed class Company : Entity
{
    // EF Core için boş constructor
    private Company()
    {
        Users = new List<User>();
    }

    public Company(string name, string taxNumber) : this()
    {
        // Base constructor (Entity) zaten Id, CreatedAt, IsActive set ediyor.
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Şirket adı boş olamaz.");
        if (string.IsNullOrWhiteSpace(taxNumber)) throw new ArgumentException("Vergi numarası boş olamaz.");
        if (taxNumber.Length != 10 && taxNumber.Length != 11) throw new ArgumentException("Vergi numarası 10 veya 11 haneli olmalıdır.");

        Name = name;
        TaxNumber = taxNumber;
    }

    public string Name { get; private set; } = default!;
    public string TaxNumber { get; private set; } = default!;
    public Address? Address { get; private set; } = default!;
    public ICollection<User> Users { get; set; }

    #region Methods
    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Şirket adı boş olamaz.");
        Name = name;
    }
    public void UpdateTaxNumber(string taxNumber)
    {
        if (string.IsNullOrWhiteSpace(taxNumber)) throw new ArgumentException("Vergi numarası boş olamaz.");
        TaxNumber = taxNumber;
    }
    #endregion
}