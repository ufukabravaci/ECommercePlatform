using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.UpdateOrderStatus)] // Veya özel bir PermissionConsts.CancelOrder
public sealed record DeleteOrderCommand(string OrderNumber) : IRequest<Result<string>>;

public sealed class DeleteOrderCommandHandler(
    IRepository<Order> orderRepository,
    IUnitOfWork unitOfWork,
    IRepository<Product> productRepository //stok iadesi için gerekli
) : IRequestHandler<DeleteOrderCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Order'ı Item'larıyla beraber çek
        // Eğer Include yapmazsak, order.Items boş gelir ve Soft Delete propagation çalışmaz.
        var order = await orderRepository.AsQueryable()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(x => x.OrderNumber == request.OrderNumber, cancellationToken);

        if (order is null)
            return Result<string>.Failure("Sipariş bulunamadı.");

        // 2. STOK İADESİ
        var productIds = order.Items.Select(i => i.ProductId).ToList();

        // Ürünleri tracking modunda çekiyoruz ki update edebilelim
        var products = await productRepository
            .WhereWithTracking(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        //orderdaki tüm itemlar için stok iadesi yap
        foreach (var item in order.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);

            // Ürün veritabanından silinmiş olabilir
            if (product != null)
            {
                // Domain Logic: Mevcut Stok + İade Edilen Adet
                product.UpdateStock(product.Stock + item.Quantity);
                productRepository.Update(product);
            }
        }

        // 3. DOMAIN LOGIC ÇAĞRISI
        // override Delete() metodu çalışır.
        // Hem Order IsDeleted=true olur, hem de OrderItems IsDeleted=true olur.
        order.Delete();

        // 2. EF Core'a Bildir
        orderRepository.Update(order);

        // 3. Kaydet
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Sipariş başarıyla iptal edildi (Silindi).");
    }
}