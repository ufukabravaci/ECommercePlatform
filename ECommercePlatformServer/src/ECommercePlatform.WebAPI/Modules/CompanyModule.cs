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
        }).RequireRateLimiting("fixed");

        // PUT: api/companies/me
        group.MapPut("me", async (ISender sender, [FromBody] UpdateCompanyCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        // DELETE: api/companies/me
        group.MapDelete("me", async (ISender sender) =>
        {
            var result = await sender.Send(new DeleteCompanyCommand());
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed");

        group.MapPut("/shipping-settings", async (
            [FromBody] UpdateShippingSettingsCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization().RequireRateLimiting("fixed");

        // 2. GET SETTINGS
        // GET /api/companies/shipping-settings
        group.MapGet("/shipping-settings", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetShippingSettingsQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireRateLimiting("fixed"); // Tenant Header'ı olduğu sürece Anonymous olabilir, ama güvenli olsun diye şimdilik Auth'lu kalsın.
    }
}
