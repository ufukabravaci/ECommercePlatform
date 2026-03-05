using ECommercePlatform.Application.Employee;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Employee;

public class GetEmployeesQueryHandlerTests
{
    private readonly Mock<ICompanyUserRepository> _companyUserRepoMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly GetEmployeesQueryHandler _handler;

    public GetEmployeesQueryHandlerTests()
    {
        _companyUserRepoMock = new Mock<ICompanyUserRepository>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new GetEmployeesQueryHandler(
            _companyUserRepoMock.Object,
            _tenantContextMock.Object
        );
    }

    private void SetUserId(User user, Guid id)
    {
        var prop = typeof(IdentityUser<Guid>).GetProperty("Id");
        prop?.SetValue(user, id);
    }

    private void SetUserNavigation(CompanyUser cu, User user)
    {
        var prop = typeof(CompanyUser).GetProperty("User");
        prop?.SetValue(cu, user);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyIdIsNull()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);

        var query = new GetEmployeesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bilgisi bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyEmployees_And_ExcludeCustomers()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        // 1. Employee (Çalışan)
        var u1 = new User("Ufuk", "Abravacı", "ufuk@test.com", "ufuk"); SetUserId(u1, Guid.NewGuid());
        var cu1 = new CompanyUser(u1.Id, companyId);
        SetUserNavigation(cu1, u1);
        cu1.AddRole("Employee"); // Çalışan rolü

        // 2. Customer (Müşteri - Listeye Girmemeli!)
        var u2 = new User("Ahmet", "Kaya", "ahmet@test.com", "ahmet"); SetUserId(u2, Guid.NewGuid());
        var cu2 = new CompanyUser(u2.Id, companyId);
        SetUserNavigation(cu2, u2);
        cu2.AddRole(RoleConsts.Customer); // Müşteri rolü

        var mockList = new List<CompanyUser> { cu1, cu2 };

        _companyUserRepoMock.Setup(x => x.GetAll()).Returns(mockList.BuildMock());

        var query = new GetEmployeesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Müşteri elenmeli, sadece çalışan gelmeli
        result.Data!.Count.Should().Be(1);
        result.Data.First().FirstName.Should().Be("Ufuk");
        result.Data.First().Roles.Should().Contain("Employee");
    }
}
