using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Users;
using GenericRepository;

//using ECommercePlatform.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Context;

public sealed class ApplicationDbContext : IdentityDbContext<User, AppRole, Guid>, IUnitOfWork
{
    private readonly IUserContext _userContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IUserContext userContext)
        : base(options)
    {
        _userContext = userContext;
    }

    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // fluentapi configlerini otomatik olarak uygular
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // 2. Global Filters (Soft Delete Otomatik Filtreleme)
        builder.ApplyGlobalFilters();

        base.OnModelCreating(builder);

        // 4. Tablo İsimlendirmeleri
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("UserRoles");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        //dbde verilerin saklanma biçimini ayarlıyoruz
        configurationBuilder.Properties<decimal>().HaveColumnType("decimal(18,2)");
        configurationBuilder.Properties<Enum>().HaveConversion<string>();

        base.ConfigureConventions(configurationBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        // Giriş yapan kullanıcının ID'sini alıyoruz
        Guid userId = _userContext.GetUserId();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.Now;

                entry.Property(x => x.IsActive).CurrentValue = true;

                if (userId != Guid.Empty)
                    entry.Property(x => x.CreatedBy).CurrentValue = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Soft Delete Kontrolü
                var isDeletedProperty = entry.Property(x => x.IsDeleted);

                if (isDeletedProperty.CurrentValue == true && isDeletedProperty.OriginalValue == false)
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
            }
            else if (entry.State == EntityState.Deleted)
            {
                throw new ArgumentException("Db'den direkt silme işlemi yapamazsınız (Soft Delete Kullanın)");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}