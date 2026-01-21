using ECommercePlatform.Application.Employee;
using Microsoft.AspNetCore.Mvc;
using TS.MediatR;

namespace ECommercePlatform.WebAPI.Modules;

public static class EmployeeModule
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees")
            .WithTags("Employees")
            .DisableAntiforgery();

        // 1. GET ALL EMPLOYEES
        // Şirket çalışanlarını listeler.
        // Yetki: Login + (PermissionConsts.ReadEmployee)
        group.MapGet("/", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Query parametresi olmadığı için direkt new() ile gönderiyoruz
            var result = await sender.Send(new GetEmployeesQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 2. SEND INVITATION
        // Yeni çalışan davet etme.
        // Yetki: Login + (PermissionConsts.InviteEmployee)
        group.MapPost("/invite", async (
            [FromBody] SendInvitationCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 3. UPDATE PERMISSIONS
        // Çalışana özel yetki verme/alma.
        // Yetki: Login + (PermissionConsts.ManagePermissions)
        group.MapPut("/permissions", async (
            [FromBody] UpdateEmployeePermissionCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 4. ACCEPT INVITATION (Mevcut Kullanıcılar İçin)
        // Zaten hesabı olan birinin davet linkine tıklayıp kabul etmesi.
        // Yetki: Login (UserContext kullanıldığı için giriş şart)
        group.MapPost("/accept-invite", async (
            [FromBody] AcceptInvitationCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 5. REGISTER EMPLOYEE (Yeni Kullanıcılar İçin)
        // Hesabı olmayan birinin davet linkiyle sıfırdan kayıt olması.
        // Yetki: PUBLIC (Token kontrolü içeride yapılıyor)
        group.MapPost("/register", async (
            [FromBody] RegisterEmployeeCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .AllowAnonymous(); // <--- ÖNEMLİ: Login gerekmez

        // 6. GET ALL SYSTEM PERMISSIONS
        // Sistemsel tüm yetki tanımlarını getirir (Checkbox listesi oluşturmak için)
        group.MapGet("/permissions-list", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAllPermissionsQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();

        // 7. GET ASSIGNABLE ROLES (Dropdown için)
        group.MapGet("/roles", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAssignableRolesQuery(), cancellationToken);
            return result.IsSuccessful ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization();
    }
}
