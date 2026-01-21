using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommercePlatform.Infrastructure.Services;

public sealed class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    public async Task<string> CreateTenantTokenAsync(
    User user,
    CompanyUser companyUser,
    CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
        new(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypesConst.CompanyId, companyUser.CompanyId.ToString()),
    };
        foreach (var role in companyUser.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in companyUser.Permissions)
        {
            claims.Add(new Claim(ClaimTypesConst.Permission, permission));
        }

        return CreateToken(claims, options.Value);
    }

    private static string CreateToken(IEnumerable<Claim> claims, JwtOptions options)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
