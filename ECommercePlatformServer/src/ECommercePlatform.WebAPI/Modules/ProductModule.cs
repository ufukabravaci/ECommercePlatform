using ECommercePlatform.Application.Products;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.WebAPI.Modules;

public static class ProductModule
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization()
            .DisableAntiforgery(); // EKLENDİ

        // CREATE
        group.MapPost("/", async (
            [FromForm] CreateProductCommand apiRequest,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(apiRequest);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).Accepts<CreateProductCommand>("multipart/form-data")
            .Produces<Result<string>>();

        // GET ALL (Pagination)
        // Örn: GET /api/products?pageNumber=1&pageSize=10&search=kalem
        group.MapGet("/", async (
            [AsParameters] GetAllProductsQuery query, // AsParameters query string'i mapler
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // UPDATE  /api/products
        group.MapPut("/", async (
            [FromBody] UpdateProductCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // DELETE /api/products/{id}
        group.MapDelete("/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
        // GET BY ID
        group.MapGet("/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.NotFound(result);
        });

        // 1. Add Image /api/products/{id}/images
        group.MapPost("/{id}/images", async (
            Guid id,
            [FromForm] IFormFile file,
            [FromForm] bool isMain,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddProductImageCommand(id, file, isMain);
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).Accepts<AddProductImageCommand>("multipart/form-data"); // Swagger için önemli

        // 2. Remove Image /api/products/{id}/images/{imageId}
        group.MapDelete("/{id}/images/{imageId}", async (
            Guid id,
            Guid imageId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveProductImageCommand(id, imageId);
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 3. Set Main Image /api/products/{id}/images/{imageId}/set-main
        group.MapPatch("/{id}/images/{imageId}/set-main", async (
            Guid id,
            Guid imageId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new SetMainProductImageCommand(id, imageId);
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}