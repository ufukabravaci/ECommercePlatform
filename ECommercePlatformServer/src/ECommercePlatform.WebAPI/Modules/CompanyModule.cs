using ECommercePlatform.Application.Companies;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class CompanyModule
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies")
            .WithTags("Companies")
            .RequireAuthorization()
            .RequireRateLimiting("fixed");

        group.MapGet("me", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCompanyQuery());
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // PUT: api/companies/me
        group.MapPut("me", async (ISender sender, [FromBody] UpdateCompanyCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // DELETE: api/companies/me
        group.MapDelete("me", async (ISender sender) =>
        {
            var result = await sender.Send(new DeleteCompanyCommand());
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}
