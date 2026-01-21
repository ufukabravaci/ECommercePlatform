using ECommercePlatform.Domain.Constants;
using System.ComponentModel;
using System.Reflection;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

public sealed record GetAllPermissionsQuery() : IRequest<Result<List<PermissionGroupDto>>>;

public sealed record PermissionGroupDto(
    string GroupName,
    string GroupLabel,
    string GroupIcon,
    List<PermissionDto> Permissions
);

public sealed record PermissionDto(
    string Code,
    string Label
);

public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionGroupDto>>>
{
    public Task<Result<List<PermissionGroupDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        // 1. PermissionConsts içindeki tüm field'ları çek (code + description)
        var permissionFields = typeof(PermissionConsts)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => new
            {
                Code = (string)fi.GetRawConstantValue()!,
                Label = fi.GetCustomAttribute<DescriptionAttribute>()?.Description
                        ?? (string)fi.GetRawConstantValue()! // Fallback: code'u kullan
            })
            .ToList();

        // 2. Gruplama yap
        var groupedPermissions = permissionFields
            .GroupBy(p => p.Code.Split('.')[0])
            .Select(g =>
            {
                var groupInfo = PermissionMetadata.GetGroupInfo(g.Key);

                return new PermissionGroupDto(
                    g.Key,
                    groupInfo.Label,
                    groupInfo.Icon,
                    g.Select(p => new PermissionDto(p.Code, p.Label)).ToList()
                );
            })
            .OrderBy(g => g.GroupLabel)
            .ToList();

        return Task.FromResult(Result<List<PermissionGroupDto>>.Succeed(groupedPermissions));
    }
}