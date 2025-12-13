using ECommercePlatform.Application.Companies;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class CompanyModule
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies")
            .RequireRateLimiting("fixed")
            .RequireAuthorization()
            .WithTags("Companies");

        group.MapPost("", async (ISender sender, CreateCompanyCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        });
    }
}
