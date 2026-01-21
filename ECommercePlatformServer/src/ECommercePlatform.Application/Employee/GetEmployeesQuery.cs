using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

[Permission(PermissionConsts.ReadEmployee)]
public sealed record GetEmployeesQuery() : IRequest<Result<List<EmployeeDto>>>;

public sealed class GetEmployeesQueryHandler(
    ICompanyUserRepository companyUserRepository,
    ITenantContext tenantContext
) : IRequestHandler<GetEmployeesQuery, Result<List<EmployeeDto>>>
{
    public async Task<Result<List<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.CompanyId is null)
            return Result<List<EmployeeDto>>.Failure("Şirket bilgisi bulunamadı.");

        // Şirkete ait CompanyUser'ları, User bilgisiyle beraber çekiyoruz
        var employees = await companyUserRepository.GetAll()
            .Include(x => x.User) // User tablosuyla Join
            .Where(x => x.CompanyId == tenantContext.CompanyId)
            .Select(x => new EmployeeDto(
                x.UserId,
                x.User.FirstName,
                x.User.LastName,
                x.User.Email!,
                x.Roles.ToList(),
                x.Permissions.ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<EmployeeDto>>.Succeed(employees);
    }
}