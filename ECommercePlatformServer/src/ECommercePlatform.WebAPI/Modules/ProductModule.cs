using ECommercePlatform.Application.Products;
using ECommercePlatform.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class ProductModule
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization()
            .DisableAntiforgery(); // EKLENDİ

        group.MapPost("/", async (
            [FromForm] CreateProductRequestApiDto apiRequest,
            ISender sender) =>
        {
            var command = new CreateProductCommand(
                apiRequest.Name,
                apiRequest.Sku,
                apiRequest.Description,
                apiRequest.Price,
                apiRequest.Currency,
                apiRequest.Stock,
                apiRequest.CategoryId,
                apiRequest.Files?.ToList()
            );

            var result = await sender.Send(command);

            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}