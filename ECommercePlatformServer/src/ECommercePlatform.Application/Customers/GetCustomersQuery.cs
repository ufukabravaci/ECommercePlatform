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
        // 1. Sorgu: CompanyUser üzerinden User'a gidiyoruz.
        // Global Filter (CompanyId) otomatik çalışır, sadece bu şirketin verileri gelir.
        var query = companyUserRepository.AsQueryable()
            .Include(cu => cu.User) // User bilgilerini çekmek için
            .Where(cu => cu.Roles.Contains(RoleConsts.Customer)); // SADECE MÜŞTERİLER

        // 2. Arama
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(cu =>
                cu.User.FirstName.Contains(request.Search) ||
                cu.User.LastName.Contains(request.Search) ||
                cu.User.Email!.Contains(request.Search));
        }

        // 3. Toplam Sayı
        int totalCount = await query.CountAsync(cancellationToken);

        // 4. Veriyi Çek ve DTO'ya Çevir (Manuel Select daha performanslıdır burda)
        var items = await query
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
            ))
            .ToListAsync(cancellationToken);

        // 5. Result
        return new PageResult<CustomerDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
