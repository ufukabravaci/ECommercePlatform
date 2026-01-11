using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.UpdateOrderStatus)] // Veya RefundOrder
public sealed record RefundOrderCommand(string OrderNumber) : IRequest<Result<string>>;

public sealed class RefundOrderCommandHandler(
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RefundOrderCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RefundOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.AsQueryable()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderNumber == request.OrderNumber, cancellationToken);

        if (order is null)
            return Result<string>.Failure("Sipariş bulunamadı.");

        if (order.Status == OrderStatus.Refunded)
            return Result<string>.Failure("Sipariş zaten iade edilmiş.");

        // 1. STOK İADESİ (Cancel ile aynı mantık)
        var productIds = order.Items.Select(x => x.ProductId).ToList();
        var products = await productRepository.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

        foreach (var item in order.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                product.UpdateStock(product.Stock + item.Quantity);
                productRepository.Update(product);
            }
        }

        // 2. STATÜ GÜNCELLEME
        // Order entity'sine bu metodu ekle: public void MarkAsRefunded() => Status = OrderStatus.Refunded;
        order.UpdateStatus(OrderStatus.Refunded);

        // 3. (Opsiyonel) Iyzico/Stripe gibi ödeme sistemi varsa "Refund API" çağrısı burada yapılır.

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Sipariş iade alındı (Stoklar güncellendi).");
    }
}