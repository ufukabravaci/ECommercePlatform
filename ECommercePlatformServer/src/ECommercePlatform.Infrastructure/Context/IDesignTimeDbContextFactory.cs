using ECommercePlatform.Application.Services;
using ECommercePlatform.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            "Data Source=.;Initial Catalog=ECommercePlatform;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");

        // ⚠ Fake contextler
        var userContext = new FakeUserContext();
        var tenantContext = new FakeTenantContext();

        return new ApplicationDbContext(
            optionsBuilder.Options,
            userContext,
            tenantContext);
    }
    private sealed class FakeUserContext : IUserContext
    {
        public Guid GetUserId()
        {
            return Guid.Empty;
        }

        public Task<bool> HasPermissionAsync(string permissionCode)
        {
            return Task.FromResult(true);
        }

        public bool IsPlatformUser()
        {
            return false;
        }

        public bool IsTenantUser()
        {
            return false;
        }
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid? CompanyId => Guid.Empty;

        public bool IsTenantScope => false;

        public bool IsPlatformScope => false;
    }
}
