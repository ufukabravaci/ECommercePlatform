using ECommercePlatform.Application.Auth;
using ECommercePlatform.Application.Auth.Login;
using ECommercePlatform.Application.Auth.RefreshToken;
using ECommercePlatform.Application.Auth.Register;
using ECommercePlatform.Application.Features.Auth.Register;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

public static class AuthModule
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth").WithTags("Authentication");

        group.MapPost("register", async (ISender sender, [FromBody] RegisterCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("fixed");

        group.MapPost("register-tenant", async (ISender sender, [FromBody] RegisterTenantCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireRateLimiting("fixed");

        group.MapPost("confirm-email", async (ISender sender, [FromBody] ConfirmEmailCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("strict");

        group.MapPost("login", async (ISender sender, [FromBody] LoginCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("strict");

        group.MapPost("login-2fa", async (ISender sender, [FromBody] LoginWithTwoFactorCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("strict");

        group.MapPost("refresh-token", async (ISender sender, [FromBody] RefreshTokenCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("fixed");

        group.MapPost("forgot-password", async (ISender sender, [FromBody] ForgotPasswordCommand command) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result); // Güvenlik gereği her zaman 200 OK
        })
        .RequireRateLimiting("strict");

        group.MapPost("reset-password", async (ISender sender, [FromBody] ResetPasswordCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireRateLimiting("strict");

        group.MapPost("revoke-all", async (ISender sender) =>
        {
            var result = await sender.Send(new RevokeAllCommand());
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireAuthorization()
        .RequireRateLimiting("fixed");

        group.MapPost("toggle-2fa", async (ISender sender, [FromBody] ToggleTwoFactorCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccessful ? Results.Ok(result) : Results.InternalServerError(result);
        })
        .RequireAuthorization()
        .RequireRateLimiting("fixed");
    }
}