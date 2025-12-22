using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Infrastructure.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace ECommercePlatform.Infrastructure;

public static class ExtensionMethods
{
    public static async Task ApplySeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await RoleSeeder.SeedAsync(scope.ServiceProvider);
        await FirstUserSeeder.SeedAsync(scope.ServiceProvider);
    }

    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Eğer entity sadece soft delete ise (Tenant değilse) burası çalışsın.
            // Tenant ise aşağıda ApplyTenantFilters ezeceği için orada ikisini birleştireceğiz.
            if (typeof(Entity).IsAssignableFrom(clrType) && !typeof(IMultiTenantEntity).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public static void ApplyTenantFilters(this ModelBuilder modelBuilder, Expression<Func<Guid?>> tenantIdExpression)
    {
        var tenantIdBody = tenantIdExpression.Body;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // 1. Sadece IMultiTenantEntity implemente edenleri bul
            if (typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var entityParam = Expression.Parameter(entityType.ClrType, "e");

                // --- A. TENANT FILTRESI OLUŞTURMA ---
                var companyIdProp = Expression.Property(entityParam, nameof(IMultiTenantEntity.CompanyId));
                var companyIdNullable = Expression.Convert(companyIdProp, typeof(Guid?));

                var isTenantIdNull = Expression.Equal(tenantIdBody, Expression.Constant(null, typeof(Guid?)));
                var idsEqual = Expression.Equal(companyIdNullable, tenantIdBody);

                // (TenantId == null || CompanyId == TenantId)
                Expression finalExpression = Expression.OrElse(isTenantIdNull, idsEqual);

                // --- B. SOFT DELETE KONTROLÜ VE BİRLEŞTİRME ---
                // Eğer bu entity AYNI ZAMANDA Soft Delete (Entity) ise, filtreye AND ekle.
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    var isDeletedProp = Expression.Property(entityParam, nameof(Entity.IsDeleted));
                    var isDeletedCondition = Expression.Equal(isDeletedProp, Expression.Constant(false));

                    // (TenantLogic) AND (IsDeleted == false)
                    finalExpression = Expression.AndAlso(finalExpression, isDeletedCondition);
                }

                var lambda = Expression.Lambda(finalExpression, entityParam);

                // Bu işlem önceki filtreyi ezer ama sorun yok çünkü Soft Delete'i içine dahil ettik.
                entityType.SetQueryFilter(lambda);
            }
        }
    }
}