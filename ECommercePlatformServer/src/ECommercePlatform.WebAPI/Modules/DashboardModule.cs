using ECommercePlatform.Application.Dashboard;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class DashboardModule
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        // GET /api/dashboard/stats
        group.MapGet("/stats", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetDashboardStatsQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}