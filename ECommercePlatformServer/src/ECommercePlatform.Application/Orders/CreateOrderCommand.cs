using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);

[Permission(PermissionConsts.CreateOrder)]
public sealed record CreateOrderCommand(
    List<CreateOrderItemDto> Items,
    string City,
    string District,
    string Street,
    string ZipCode,
    string FullAddress
) : IRequest<Result<string>>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Sepet boş olamaz.");
        RuleFor(x => x.City).NotEmpty().WithMessage("Şehir zorunludur.");
        RuleFor(x => x.District).NotEmpty().WithMessage("İlçe zorunludur.");
        RuleFor(x => x.Street).NotEmpty().WithMessage("Sokak zorunludur.");
        RuleFor(x => x.FullAddress).NotEmpty().WithMessage("Tam adres zorunludur.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId).NotEmpty();
            items.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

// 4. HANDLER
public sealed class CreateOrderCommandHandler(
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository,
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,    // Kullanıcı ID'si için
    ITenantContext tenantContext // Şirket ID'si için
) : IRequestHandler<CreateOrderCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // A. Context Verilerini Al
        // Not: PermissionBehavior zaten yetki ve tenant kontrolünü yaptı.
        Guid userId = userContext.GetUserId();
        Guid companyId = tenantContext.CompanyId!.Value;

        // B. Ürünleri ve Stokları Getir
        // Global Query Filter (CompanyId) otomatik çalışır.
        var productIds = request.Items.Select(x => x.ProductId).ToList();

        var products = await productRepository
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Distinct().Count())
            return Result<string>.Failure("Sepetteki bazı ürünler bulunamadı veya satıştan kaldırıldı.");

        var firstCurrency = products.First().Price.Currency;
        if (products.Any(p => p.Price.Currency != firstCurrency))
        {
            return Result<string>.Failure("Sepetinizde farklı para birimlerine sahip ürünler var. Lütfen kontrol ediniz.");
        }

        // C. Stok düşümü ve Order Hazırlığı
        var shippingAddress = new Address(
            request.City,
            request.District,
            request.Street,
            request.ZipCode,
            request.FullAddress);
        var order = new Order(userId, companyId, shippingAddress);

        foreach (var itemDto in request.Items)
        {
            var product = products.First(p => p.Id == itemDto.ProductId);

            if (product.Stock < itemDto.Quantity)
            {
                return Result<string>.Failure($"{product.Name} için yeterli stok yok. (Mevcut: {product.Stock})");
            }

            // Domain Behavior
            product.UpdateStock(product.Stock - itemDto.Quantity);
            productRepository.Update(product);

            order.AddOrderItem(product.Id, product.Name, product.Price, itemDto.Quantity);
        }

        // D. Kaydet (Transactional)
        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // E. Sepeti Temizle
        await basketRepository.DeleteBasketAsync(userId, cancellationToken);


        return Result<string>.Succeed(order.OrderNumber);
    }
}
