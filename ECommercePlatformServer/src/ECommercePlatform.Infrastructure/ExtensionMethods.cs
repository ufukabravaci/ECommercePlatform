using ECommercePlatform.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommercePlatform.Infrastructure;

public static class ExtensionMethods
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            //Clr => Common Language Runtime. Her tabloya karşılık gelen entitiynin c#taki sınıf karşılığı.
            var clrType = entityType.ClrType;
            //Eğer bir class entity tipinden türemişse onun için filtre uygulanır. Yani tüm entityler için.
            if (typeof(Entity).IsAssignableFrom(clrType))
            {
                //e => e.IsDeleted == false Ama bu User,Car gibi her tip için runtime'da otomatik oluşturulur.
                //yani isdeleted == true olanlar dönmeyecek filtrelenecek. Her sorguya IsDeleted == false diye eklemeyeceğiz.
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }


    //e => _tenantContext.GetCompanyId() == null || (Guid?) e.CompanyId == _tenantContext.GetCompanyId()
    //    SELECT* FROM Products
    //      WHERE IsDeleted = 0  -- Soft Delete Filtresi
    //      AND(
    //    (@__tenantId_0 IS NULL) -- Eğer Token'da ID yoksa (Admin) burası TRUE olur, hepsini getirir.
    //    OR
    //    (CompanyId = @__tenantId_0) -- Token'da ID varsa, sadece o şirketin verisi gelir.
    //)
    public static void ApplyTenantFilters(this ModelBuilder modelBuilder, Expression<Func<Guid?>> tenantIdExpression)
    {
        // 1. Tenant ID'yi veren expression'ın gövdesini alıyoruz (Run-time'da çalışacak kısım)
        // Bu gövde şuna denk gelir: "_tenantContext.GetCompanyId()"
        var tenantIdBody = tenantIdExpression.Body;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // 2. Sadece IMultiTenantEntity implemente edenleri bul
            if (typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Parametre: "e" => Product e
                var entityParam = Expression.Parameter(entityType.ClrType, "e");

                // Property: "e.CompanyId"
                var companyIdProp = Expression.Property(entityParam, nameof(IMultiTenantEntity.CompanyId));

                // DİKKAT: CompanyId (Guid) ile GetCompanyId (Guid?) karşılaştırması için tür dönüşümü lazım.
                // SQL karşılığı: (Guid?)e.CompanyId
                var companyIdNullable = Expression.Convert(companyIdProp, typeof(Guid?));

                // ŞART 1: TenantID null mu? (Admin durumu)
                // Expression: _tenantContext.GetCompanyId() == null
                var isTenantIdNull = Expression.Equal(tenantIdBody, Expression.Constant(null, typeof(Guid?)));

                // ŞART 2: ID'ler eşit mi?
                // Expression: (Guid?)e.CompanyId == _tenantContext.GetCompanyId()
                var idsEqual = Expression.Equal(companyIdNullable, tenantIdBody);

                // OR işlemi: (TenantId == null) OR (IdsEqual)
                var finalCondition = Expression.OrElse(isTenantIdNull, idsEqual);

                // Lambda'yı derle: e => ...
                var lambda = Expression.Lambda(finalCondition, entityParam);

                // Filtreyi uygula
                entityType.SetQueryFilter(lambda);
            }
        }
    }

}