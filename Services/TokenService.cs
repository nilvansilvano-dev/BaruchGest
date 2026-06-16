using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime Expira) GenerateToken(Usuario usuario)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        };

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           expira,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
