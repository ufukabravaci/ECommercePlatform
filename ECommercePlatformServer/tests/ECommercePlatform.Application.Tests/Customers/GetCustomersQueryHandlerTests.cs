using ECommercePlatform.Application.Customers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Customers;

public class GetCustomersQueryHandlerTests
{
    private readonly Mock<IRepository<CompanyUser>> _companyUserRepositoryMock;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _companyUserRepositoryMock = new Mock<IRepository<CompanyUser>>();
        _handler = new GetCustomersQueryHandler(_companyUserRepositoryMock.Object);
    }

    // IdentityUser Id ataması
    private void SetUserId(User user, Guid id)
    {
        var prop = typeof(IdentityUser<Guid>).GetProperty("Id");
        prop?.SetValue(user, id);
    }

    // CompanyUser'ın içindeki User'ı Reflection ile atama
    private void SetUserNavigation(CompanyUser cu, User user)
    {
        var prop = typeof(CompanyUser).GetProperty("User");
        prop?.SetValue(cu, user);
    }

    // CompanyUser içindeki özel (private/IReadOnly) Roles listesini atama
    private void AddRoleToCompanyUser(CompanyUser cu, string role)
    {
        // Entity içerisinde private readonly List<string> _roles gibi bir field olabilir
        // veya public ICollection<string> şeklinde olabilir. 
        // Eğer domain'de AddRole() gibi bir metod varsa direkt onu çağırabilirsin. 
        // Biz burada güvence olarak List<string> property'sini veya arkaplandaki alanı arayacağız.

        var type = typeof(CompanyUser);

        // Eğer public bir "Roles" var ve tipi List ise:
        var rolesProp = type.GetProperty("Roles");
        if (rolesProp != null && rolesProp.CanWrite)
        {
            var list = new List<string> { role };
            rolesProp.SetValue(cu, list);
            return;
        }

        // Eğer Roles salt okunur ise (IReadOnlyCollection vs), muhtemelen backing field (ör: _roles) vardır.
        var backingField = type.GetField("_roles", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField != null)
        {
            var list = (List<string>?)backingField.GetValue(cu);
            list?.Add(role);
        }
        else
        {
            // Eğer record type veya farklı bir yapıysa List'i direkt yansıtmayı deneyelim
            var currentRoles = rolesProp?.GetValue(cu) as IEnumerable<string>;
            var newList = currentRoles != null ? currentRoles.ToList() : new List<string>();
            newList.Add(role);

            // Eğer IReadOnlyCollection ise, onu atamaya çalışalım (eğer set edilebilir backing yoksa, ki bu zordur, bu yüzden backing field genelde çalışır)
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyCustomers_And_SupportSearch()
    {
        // Arrange
        var u1Id = Guid.NewGuid();
        var u2Id = Guid.NewGuid();
        var u3Id = Guid.NewGuid();

        var user1 = new User("Ahmet", "Yılmaz", "ahmet@test.com", "ahmet"); SetUserId(user1, u1Id);
        var user2 = new User("Ayşe", "Kaya", "ayse@test.com", "ayse"); SetUserId(user2, u2Id);
        var adminUser = new User("Admin", "Bey", "admin@test.com", "admin"); SetUserId(adminUser, u3Id);

        var cu1 = new CompanyUser(u1Id, Guid.NewGuid()); SetUserNavigation(cu1, user1);
        AddRoleToCompanyUser(cu1, RoleConsts.Customer);

        var cu2 = new CompanyUser(u2Id, Guid.NewGuid()); SetUserNavigation(cu2, user2);
        AddRoleToCompanyUser(cu2, RoleConsts.Customer);

        var cuAdmin = new CompanyUser(u3Id, Guid.NewGuid()); SetUserNavigation(cuAdmin, adminUser);
        AddRoleToCompanyUser(cuAdmin, "Admin");

        var data = new List<CompanyUser> { cu1, cu2, cuAdmin };

        _companyUserRepositoryMock.Setup(x => x.AsQueryable()).Returns(data.BuildMock());

        var query = new GetCustomersQuery(1, 10, "ayşe");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().FirstName.Should().Be("Ayşe");
    }
}
