using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Dashboard;

public sealed record GetDashboardStatsQuery() : IRequest<Result<DashboardStatsDto>>;

public sealed class GetDashboardStatsQueryHandler(
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<Brand> brandRepository,
    IRepository<Review> reviewRepository,
    IRepository<CompanyUser> companyUserRepository
) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        // ==================== SİPARİŞ İSTATİSTİKLERİ ====================
        var ordersQuery = orderRepository.AsQueryable();

        var totalOrders = await ordersQuery.CountAsync(cancellationToken);

        var orderStatusCounts = await ordersQuery
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Revenue (Teslim edilenlerin Money.Amount * Quantity toplamı)
        var totalRevenue = await ordersQuery
            .Where(o => o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .SumAsync(item => item.Price.Amount * item.Quantity, cancellationToken);

        // Son 5 Sipariş
        var recentOrders = await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ProjectToType<DashboardRecentOrderDto>()
            .ToListAsync(cancellationToken);

        // ==================== ÜRÜN İSTATİSTİKLERİ ====================
        var productsQuery = productRepository.AsQueryable();

        var totalProducts = await productsQuery.CountAsync(cancellationToken);
        var inStockProducts = await productsQuery.CountAsync(p => p.Stock > 10, cancellationToken);
        var lowStockProductsCount = await productsQuery.CountAsync(p => p.Stock > 0 && p.Stock <= 10, cancellationToken);
        var outOfStockProducts = await productsQuery.CountAsync(p => p.Stock <= 0, cancellationToken);

        // Düşük Stoklu Ürünler (Mapster ProjectToType Kullanımı)
        var lowStockProductsList = await productsQuery
            .Where(p => p.Stock <= 10 && p.Stock > 0)
            .OrderBy(p => p.Stock)
            .Take(5)
            .ProjectToType<DashboardLowStockProductDto>()
            .ToListAsync(cancellationToken);

        // ==================== DİĞERLERİ ====================
        var totalCategories = await categoryRepository.AsQueryable().CountAsync(cancellationToken);
        var totalBrands = await brandRepository.AsQueryable().CountAsync(cancellationToken);
        // 1. Önce şirkete ait tüm CompanyUser'ları çekiyoruz.
        var currentCompanyUsers = await companyUserRepository.AsQueryable()
            .ToListAsync(cancellationToken);
        // 2. Bellekte (In-memory) müşteri rolüne sahip olanları sayıyoruz.
        var totalCustomers = currentCompanyUsers
            .Count(cu => cu.Roles.Contains(RoleConsts.Customer));

        // ==================== YORUMLAR ====================
        var reviewsQuery = reviewRepository.AsQueryable();
        var totalReviews = await reviewsQuery.CountAsync(cancellationToken);
        var averageRating = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => r.Rating, cancellationToken) : 0;

        return new DashboardStatsDto
        {
            TotalOrders = totalOrders,
            PendingOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Pending)?.Count ?? 0,
            ConfirmedOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Confirmed)?.Count ?? 0,
            ProcessingOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Processing)?.Count ?? 0,
            ShippedOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Shipped)?.Count ?? 0,
            DeliveredOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Delivered)?.Count ?? 0,
            CancelledOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Cancelled)?.Count ?? 0,
            RefundedOrders = orderStatusCounts.FirstOrDefault(x => x.Status == OrderStatus.Refunded)?.Count ?? 0,
            TotalRevenue = totalRevenue,
            RecentOrders = recentOrders,
            TotalProducts = totalProducts,
            InStockProducts = inStockProducts,
            LowStockProducts = lowStockProductsCount,
            OutOfStockProducts = outOfStockProducts,
            LowStockProductsList = lowStockProductsList,
            TotalCustomers = totalCustomers,
            TotalCategories = totalCategories,
            TotalBrands = totalBrands,
            TotalReviews = totalReviews,
            ApprovedReviews = await reviewsQuery.CountAsync(r => r.IsApproved, cancellationToken),
            PendingReviews = await reviewsQuery.CountAsync(r => !r.IsApproved, cancellationToken),
            AverageRating = Math.Round(averageRating, 1)
        };
    }
}