using ECommercePlatform.Application.Employee;
using FluentAssertions;

namespace ECommercePlatform.Application.Tests.Employee;

public class GetAllPermissionsQueryHandlerTests
{
    private readonly GetAllPermissionsQueryHandler _handler;

    public GetAllPermissionsQueryHandlerTests()
    {
        _handler = new GetAllPermissionsQueryHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnGroupedPermissions()
    {
        var query = new GetAllPermissionsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // E-Ticaret sisteminde en az 1-2 tane permission grubu kesinlikle olmalı (örn: "Products", "Orders" vb.)
        result.Data!.Should().NotBeEmpty();

        var firstGroup = result.Data.First();
        firstGroup.GroupName.Should().NotBeNullOrWhiteSpace();
        firstGroup.Permissions.Should().NotBeEmpty();
    }
}
