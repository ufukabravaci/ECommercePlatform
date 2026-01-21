using ECommercePlatform.Domain.Constants;
using System.ComponentModel;
using System.Reflection;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

public sealed record GetAssignableRolesQuery() : IRequest<Result<List<RoleDto>>>;

public sealed record RoleDto(
    string Code,    // "Employee"
    string Label    // "Çalışan"
);

public sealed class GetAssignableRolesQueryHandler : IRequestHandler<GetAssignableRolesQuery, Result<List<RoleDto>>>
{
    // Atanması yasak roller
    private static readonly HashSet<string> ExcludedRoles = new()
    {
        RoleConsts.SuperAdmin,
        RoleConsts.Customer,
        RoleConsts.CompanyOwner
    };

    public Task<Result<List<RoleDto>>> Handle(GetAssignableRolesQuery request, CancellationToken cancellationToken)
    {
        // 1. RoleConsts içindeki tüm const field'ları bul
        var roleFields = typeof(RoleConsts)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .ToList();

        // 2. Rol listesi oluştur (code + label)
        var assignableRoles = roleFields
            .Select(fi => new
            {
                Code = (string)fi.GetRawConstantValue()!,
                Label = fi.GetCustomAttribute<DescriptionAttribute>()?.Description
                        ?? (string)fi.GetRawConstantValue()!
            })
            .Where(r => !ExcludedRoles.Contains(r.Code))
            .Select(r => new RoleDto(r.Code, r.Label))
            .OrderBy(r => r.Label)
            .ToList();

        return Task.FromResult(Result<List<RoleDto>>.Succeed(assignableRoles));
    }
}
