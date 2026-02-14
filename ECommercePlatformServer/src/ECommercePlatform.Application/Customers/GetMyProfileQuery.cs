using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Profile;

public sealed record GetMyProfileQuery() : IRequest<Result<UserProfileDto>>;

public sealed class GetMyProfileQueryHandler(
    UserManager<User> userManager,
    IUserContext userContext
) : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userIdString = userContext.GetUserId().ToString();
        if (string.IsNullOrEmpty(userIdString))
        {
            return Result<UserProfileDto>.Failure("Kullanıcı bulunamadı.");
        }

        var user = await userManager.FindByIdAsync(userIdString);
        if (user is null)
        {
            return Result<UserProfileDto>.Failure("Kullanıcı veritabanında bulunamadı.");
        }

        var addressDto = user.Address != null
            ? new AddressDto(user.Address.City, user.Address.District, user.Address.Street, user.Address.ZipCode, user.Address.FullAddress)
            : null;

        var profileDto = new UserProfileDto(
            user.FirstName,
            user.LastName,
            user.Email!,
            user.UserName!,
            user.PhoneNumber,
            addressDto
        );

        return Result<UserProfileDto>.Succeed(profileDto);
    }
}