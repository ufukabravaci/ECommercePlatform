using ECommercePlatform.Application.Reviews;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class ReviewModule
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews")
            .WithTags("Reviews")
            .DisableAntiforgery();
        // TÜM YORUMLAR
        group.MapGet("/", async (
            [AsParameters] GetAllReviewsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");

        // 1. CREATE (Müşteri)
        group.MapPost("/", async (
            [FromBody] CreateReviewCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed"); // Token şart

        // 2. REPLY (Satıcı)
        group.MapPost("/{id}/reply", async (
        Guid id,
        [FromBody] string reply, // Düz string alıyoruz
        ISender sender,
        CancellationToken cancellationToken) =>
        {
            // Command'i burada oluşturuyoruz
            var command = new ReplyReviewCommand(id, reply);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");

        // 3. GET BY PRODUCT (Public - Ürün Detay)
        group.MapGet("/product/{productId}", async (
            Guid productId,
            [AsParameters] GetProductReviewsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Query parametresindeki ProductId'yi route'dan gelenle ez
            query = query with { ProductId = productId };
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        }).RequireRateLimiting("fixed");

        // 4. APPROVE (Satıcı)
        group.MapPatch("/{id}/approve", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ApproveReviewCommand(id), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");

        // 5. REJECT (Satıcı)
        group.MapPatch("/{id}/reject", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RejectReviewCommand(id), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");

        group.MapDelete("/{id}", async (
        Guid id,
        ISender sender,
        CancellationToken cancellationToken) =>
        {
            var command = new DeleteReviewCommand(id);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");
    }
}
