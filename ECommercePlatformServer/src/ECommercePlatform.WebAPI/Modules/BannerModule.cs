using ECommercePlatform.Application.Banners;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class BannerModule
{
    public static void MapBannerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/banners")
            .WithTags("Banners")
            .RequireAuthorization()
            .DisableAntiforgery(); // Dosya yükleme için gerekli

        // 1. CREATE (Form Data)
        group.MapPost("/", async (
            [FromForm] CreateBannerCommand command, // Form'dan gelecek
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).Accepts<CreateBannerCommand>("multipart/form-data").RequireRateLimiting("fixed");

        // 2. UPDATE (Form Data)
        group.MapPut("/", async (
            [FromForm] UpdateBannerCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).Accepts<UpdateBannerCommand>("multipart/form-data").RequireRateLimiting("fixed");

        // 3. DELETE
        group.MapDelete("/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteBannerCommand(id), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // 4. GET ALL
        group.MapGet("/", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAllBannersQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");
    }
}