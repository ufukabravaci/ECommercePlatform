using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Auth;

public sealed record ToggleTwoFactorCommand(bool Enable) : IRequest<Result<string>>;

public sealed class ToggleTwoFactorCommandHandler(
    IUserContext userContext,
    UserManager<User> userManager) : IRequestHandler<ToggleTwoFactorCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ToggleTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı.");

        var result = await userManager.SetTwoFactorEnabledAsync(user, request.Enable);

        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());

        string status = request.Enable ? "aktif edildi" : "pasif edildi";
        return $"İki aşamalı doğrulama başarıyla {status}.";
    }
}
