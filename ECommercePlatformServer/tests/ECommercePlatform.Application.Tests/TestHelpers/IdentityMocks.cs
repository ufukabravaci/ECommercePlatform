using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommercePlatform.Application.Tests.TestHelpers;

public static class IdentityMocks
{
    public static Mock<UserManager<TUser>> CreateMockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<TUser>>();
        var userValidators = new List<IUserValidator<TUser>>();
        var pwdValidators = new List<IPasswordValidator<TUser>>();
        var normalizer = new Mock<ILookupNormalizer>();
        var describer = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<TUser>>>();

        return new Mock<UserManager<TUser>>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            pwdValidators,
            normalizer.Object,
            describer,
            services.Object,
            logger.Object
        );
    }

    public static Mock<RoleManager<TRole>> CreateMockRoleManager<TRole>() where TRole : class
    {
        var roleStore = new Mock<IRoleStore<TRole>>();
        var roleValidators = new List<IRoleValidator<TRole>>();
        var normalizer = new Mock<ILookupNormalizer>();
        var identityErrorDescriber = new IdentityErrorDescriber();
        var logger = new Mock<ILogger<RoleManager<TRole>>>();

        return new Mock<RoleManager<TRole>>(
            roleStore.Object,
            roleValidators,
            normalizer.Object,
            identityErrorDescriber,
            logger.Object
        );
    }

}