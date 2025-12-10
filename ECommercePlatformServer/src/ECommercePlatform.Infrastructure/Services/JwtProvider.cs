using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommercePlatform.Infrastructure.Services;

public sealed class JwtProvider(IOptions<JwtOptions> _options) : IJwtProvider
{
    public async Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the token
        };
        // claims.Add(new Claim("Role", "Admin"));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.Value.AccessTokenExpirationMinutes),
            Issuer = _options.Value.Issuer,
            Audience = _options.Value.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create(); //unpredictable random number generator
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}