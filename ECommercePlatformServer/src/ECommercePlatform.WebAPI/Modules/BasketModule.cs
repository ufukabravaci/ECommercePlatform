using ECommercePlatform.Application.Baskets;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class BasketModule
{
    public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/baskets")
            .WithTags("Baskets")
            .RequireAuthorization() // Sadece giriş yapmış kullanıcılar
            .DisableAntiforgery();

        // 1. GET BASKET
        group.MapGet("/", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetBasketQuery(), cancellationToken);
            return Results.Ok(result);
        });

        // 2. UPDATE BASKET (Add/Remove Item mantığı burada döner)
        // Body: { "items": [ { "productId": "...", "quantity": 1, ... } ] }
        group.MapPost("/", async (
            [FromBody] UpdateBasketCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 3. DELETE BASKET
        group.MapDelete("/", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteBasketCommand(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}