// WebAPI/Modules/ProfileModule.cs
using ECommercePlatform.Application.Profile;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class ProfileModule
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile")
            .RequireAuthorization()
            .DisableAntiforgery();

        // GET ME
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyProfileQuery(), ct);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });

        // UPDATE ME
        group.MapPut("/", async (UpdateMyProfileCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}
