using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CityMove.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CityMove.API.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public (string token, DateTime expiraEm) GerarToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"] ?? "240"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.Nome),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
