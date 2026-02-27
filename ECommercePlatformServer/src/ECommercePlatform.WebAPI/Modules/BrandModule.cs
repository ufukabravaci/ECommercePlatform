using ECommercePlatform.Application.Brands;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class BrandModule
{
    public static void MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands")
            .WithTags("Brands")
            .RequireAuthorization()
            .DisableAntiforgery();

        // 1. CREATE
        group.MapPost("/", async (
            [FromBody] CreateBrandCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // 2. UPDATE
        group.MapPut("/", async (
            [FromBody] UpdateBrandCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // 3. DELETE
        group.MapDelete("/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteBrandCommand(id), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // 4. GET ALL
        group.MapGet("/", async (
            [AsParameters] GetAllBrandsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");
    }
}
