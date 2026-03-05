using ECommercePlatform.Application.Employee;
using ECommercePlatform.Domain.Constants; // RoleConsts için gerekli
using FluentAssertions;

namespace ECommercePlatform.Application.Tests.Employee;

public class GetAssignableRolesQueryHandlerTests
{
    private readonly GetAssignableRolesQueryHandler _handler;

    public GetAssignableRolesQueryHandlerTests()
    {
        _handler = new GetAssignableRolesQueryHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnAssignableRoles_WithoutExcludedRoles()
    {
        var query = new GetAssignableRolesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Data içinde "SuperAdmin", "Customer" veya "CompanyOwner" olMAMALI
        result.Data!.Should().NotContain(r => r.Code == RoleConsts.SuperAdmin);
        result.Data.Should().NotContain(r => r.Code == RoleConsts.Customer);
        result.Data.Should().NotContain(r => r.Code == RoleConsts.CompanyOwner);

        // Sisteme bağlı olarak en az 1 tane atanabilir rol (Örn: Employee, Manager vb) olması beklenir.
        result.Data.Should().NotBeEmpty();
    }
}
