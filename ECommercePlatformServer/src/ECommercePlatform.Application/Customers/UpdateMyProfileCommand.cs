// Application/Profile/UpdateMyProfileCommand.cs
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Profile;

public sealed record UpdateMyProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    AddressDto? Address
) : IRequest<Result<string>>;

public sealed class UpdateMyProfileCommandHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<UpdateMyProfileCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Result<string>.Failure("Kullanıcı oturumu geçersiz.");
        }

        var user = await userManager.FindByIdAsync(userIdString);
        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı.");
        }

        try
        {
            user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
        }
        catch (ArgumentException ex)
        {
            // Domain validation hatası varsa yakala
            return Result<string>.Failure(ex.Message);
        }


        if (request.Address != null)
        {
            var newAddress = new Address(
                request.Address.City,
                request.Address.District,
                request.Address.Street,
                request.Address.ZipCode,
                request.Address.FullAddress);

            user.SetAddress(newAddress);
        }

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Result<string>.Succeed("Profil başarıyla güncellendi.");
    }
}
