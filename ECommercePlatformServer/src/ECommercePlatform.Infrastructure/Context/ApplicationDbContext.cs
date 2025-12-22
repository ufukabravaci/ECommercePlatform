using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using GenericRepository;

//using ECommercePlatform.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Context;

public sealed class ApplicationDbContext : IdentityDbContext<User, AppRole, Guid>, IUnitOfWork
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IUserContext userContext,
        ITenantContext tenantContext)
        : base(options)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
    }

    #region DbSets

    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();

    #endregion

    #region Model Configuration

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Fluent API Configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.ApplyGlobalFilters();
        builder.ApplyTenantFilters(() => _tenantContext.CompanyId);

        base.OnModelCreating(builder);

        // 4️⃣ Identity Table Names
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>()
               .ToTable("UserClaims");

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>()
               .ToTable("UserLogins");

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>()
               .ToTable("UserTokens");

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>()
               .ToTable("RoleClaims");

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>()
               .ToTable("UserRoles");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
            .HaveColumnType("decimal(18,2)");

        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>();

        base.ConfigureConventions(configurationBuilder);
    }

    #endregion

    #region SaveChanges (Auditing + Soft Delete)

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        Guid userId = _userContext.GetUserId();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.Now;

                    entry.Property(x => x.IsActive).CurrentValue = true;

                    if (userId != Guid.Empty)
                        entry.Property(x => x.CreatedBy).CurrentValue = userId;

                    break;

                case EntityState.Modified:
                    var isDeleted = entry.Property(x => x.IsDeleted);

                    if (isDeleted.CurrentValue == true &&
                        isDeleted.OriginalValue == false)
                    {
                        entry.Property(x => x.DeletedAt).CurrentValue = DateTimeOffset.Now;

                        if (userId != Guid.Empty)
                            entry.Property(x => x.DeletedBy).CurrentValue = userId;
                    }
                    else
                    {
                        entry.Property(x => x.UpdatedAt).CurrentValue = DateTimeOffset.Now;

                        if (userId != Guid.Empty)
                            entry.Property(x => x.UpdatedBy).CurrentValue = userId;
                    }
                    break;

                case EntityState.Deleted:
                    throw new InvalidOperationException(
                        "Hard delete yasak. Soft delete kullanmalısınız.");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    #endregion
}