using ECommercePlatform.Application.Customers;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class CustomerModule
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .RequireAuthorization()
            .DisableAntiforgery();

        // GET ALL (Pagination)
        group.MapGet("/", async (
            [AsParameters] GetCustomersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // DELETE CUSTOMER (Şirketten Çıkar)
        group.MapDelete("/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveCustomerCommand(id), ct);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");
    }
}
