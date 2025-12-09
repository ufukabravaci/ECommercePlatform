namespace ECommercePlatform.WebAPI.Modules;

public static class AuthModule
{
    public static void RegisterAuthRoutes(this IEndpointRouteBuilder app)
    {
        // Grup Tanımı: /api/auth
        var group = app.MapGroup("/api/auth").RequireRateLimiting("fixed"); // Rate Limit'i gruba uygula

        // 1. Login Endpoint (Şimdilik Dummy)
        group.MapPost("/login", () =>
        {
            return Results.Ok(new { message = "Login endpoint çalışıyor" });
        });

        // 2. Register Endpoint (Şimdilik Dummy)
        group.MapPost("/register", () =>
        {
            return Results.Ok(new { message = "Register endpoint çalışıyor" });
        });
    }
}