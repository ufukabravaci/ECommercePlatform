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

        group.MapPost("/", async (
            [FromForm] CreateProductCommand apiRequest,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(apiRequest);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).Accepts<CreateProductCommand>("multipart/form-data")
            .Produces<Result<string>>();
    }
}