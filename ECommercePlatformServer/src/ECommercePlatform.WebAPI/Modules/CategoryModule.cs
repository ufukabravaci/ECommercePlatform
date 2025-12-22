using ECommercePlatform.Application.Categories;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class CategoryModule
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .RequireAuthorization()
            .RequireRateLimiting("fixed");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllCategoriesQuery());
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/", async (ISender sender, [FromBody] CreateCategoryCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // id command içinde var
        group.MapPut("/", async (ISender sender, [FromBody] UpdateCategoryCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        //soft delete
        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteCategoryCommand(id));
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}
