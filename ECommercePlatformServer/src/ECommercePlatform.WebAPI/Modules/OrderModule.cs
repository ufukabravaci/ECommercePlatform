using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class OrderModule
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization() // Tüm endpointler için Token şart
            .DisableAntiforgery();

        // 1. CREATE ORDER (Müşteri / Personel)
        // Body: { Items: [...], City: "...", ... }
        group.MapPost("/", async (
            [FromBody] CreateOrderCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireRateLimiting("fixed");

        // 2. GET ALL ORDERS (Mağaza Sahibi / Personel)
        // Query: ?pageNumber=1&pageSize=10&search=ORD-123
        group.MapGet("/", async (
            [AsParameters] GetAllOrdersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 3. GET MY ORDERS (Müşteri)
        // Query: ?pageNumber=1&pageSize=10
        // Sadece giriş yapan kullanıcının siparişlerini getirir.
        group.MapGet("/my-orders", async (
            [AsParameters] GetMyOrdersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 4. GET ORDER BY ID (Ortak)
        // URL: /api/orders/{id}
        group.MapGet("/{orderNumber}", async (
            string orderNumber,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(orderNumber), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.NotFound(result);
        });

        // 5. UPDATE STATUS (Mağaza Sahibi / Personel)
        // URL: /api/orders/{id}/status
        // Body (Raw JSON): 1  (veya 2, 3 gibi Enum integer değeri)
        group.MapPatch("/{orderNumber}/status", async (
            string orderNumber,
            [FromBody] int status,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Enum kontrolü validatörde yapılacak
            var command = new UpdateOrderStatusCommand(orderNumber, (OrderStatus)status);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 6. DELETE / CANCEL ORDER (Mağaza Sahibi / Personel)
        // URL: /api/orders/{id}
        group.MapDelete("/{orderNumber}", async (
            string orderNumber,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteOrderCommand(orderNumber);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 7-) REFUND
        group.MapPatch("/{orderNumber}/refund", async (
            string orderNumber,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RefundOrderCommand(orderNumber), ct);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 8-) TRACKING NUMBER EKLEME
        // Body: { "trackingNumber": "123456" }
        group.MapPatch("/{orderNumber}/tracking", async (
            string orderNumber,
            [FromBody] string trackingNumber,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new AddTrackingNumberCommand(orderNumber, trackingNumber);
            var result = await sender.Send(command, ct);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}