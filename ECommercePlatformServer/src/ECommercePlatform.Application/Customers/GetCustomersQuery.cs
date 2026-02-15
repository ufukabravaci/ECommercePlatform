using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Customers;

[Permission(PermissionConsts.ReadCustomer)]
public sealed record GetCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string Search = ""
) : IRequest<Result<PageResult<CustomerDto>>>;

public sealed class GetCustomersQueryHandler(
    IRepository<CompanyUser> companyUserRepository
) : IRequestHandler<GetCustomersQuery, Result<PageResult<CustomerDto>>>
{
    public async Task<Result<PageResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        // 1. ADIM: Dashboard'da yaptığın gibi veriyi ÖNCE çekiyoruz.
        // Include(cu => cu.User) ile User bilgilerini de aldık.
        var allCompanyUsers = await companyUserRepository.AsQueryable()
            .Include(cu => cu.User)
            .ToListAsync(cancellationToken);

        // 2. ADIM: Bellekte (C# tarafında) filtreleme yapıyoruz.
        // Dashboard'daki .Contains(RoleConsts.Customer) mantığı burada devreye giriyor.
        var customerList = allCompanyUsers
            .Where(cu => cu.Roles.Contains(RoleConsts.Customer))
            .ToList();

        // 3. ADIM: Arama filtresi (Search)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            customerList = customerList.Where(cu =>
                (cu.User.FirstName + " " + cu.User.LastName).ToLower().Contains(search) ||
                cu.User.Email!.ToLower().Contains(search)
            ).ToList();
        }

        // 4. ADIM: Toplam Sayı ve Sayfalama
        int totalCount = customerList.Count;

        var items = customerList
            .OrderByDescending(cu => cu.User.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(cu => new CustomerDto(
                cu.User.Id,
                cu.User.FirstName,
                cu.User.LastName,
                cu.User.Email!,
                cu.User.CreatedAt,
                cu.User.IsActive
            )).ToList();

        // 5. ADIM: Sayfalanmış Sonucu Dön
        return new PageResult<CustomerDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
