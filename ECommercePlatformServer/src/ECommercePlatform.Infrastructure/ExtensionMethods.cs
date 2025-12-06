using ECommercePlatform.Core.Abstractions;
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
}