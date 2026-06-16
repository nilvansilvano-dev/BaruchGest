using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Services;

namespace FinanceiroAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FinanceiroDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(FinanceiroDbContext db, TokenService tokenService)
    {
        _db           = db;
        _tokenService = tokenService;
    }

    /// <summary>Login. Perfis: "usuario" (leitura + escrita) | "contador" (somente leitura).</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Senha, usuario.SenhaHash))
            return Unauthorized("Email ou senha incorretos.");

        if (!usuario.Ativo)
            return Unauthorized("Conta desativada. Entre em contato com o contador.");

        var (token, expira) = _tokenService.GenerateToken(usuario);
        return Ok(new LoginResponse(token, usuario.Perfil, expira));
    }
}
