using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Models;
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
        return Ok(new LoginResponse(token, usuario.Perfil, expira, usuario.Id));
    }

    /// <summary>Registro público. Perfil sempre "usuario" — conta de contador requer criação pelo admin.</summary>
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroRequest req)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email já cadastrado.");

        var usuario = new Usuario
        {
            Nome          = req.Nome,
            Email         = req.Email,
            SenhaHash     = BCrypt.Net.BCrypt.HashPassword(req.Senha),
            Perfil        = "usuario",
            Ativo         = true,
            Telefone      = req.Telefone,
            TipoDocumento = req.TipoDocumento,
            Documento     = req.Documento,
            Endereco      = req.Endereco,
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Created("", ToResponse(usuario));
    }

    /// <summary>Redefine a senha pelo email (sistema local sem email de verificação).</summary>
    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest req)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (usuario is null)
            return NotFound("Email não encontrado.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.NovaSenha);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static UsuarioResponse ToResponse(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Ativo, u.CriadoEm,
            u.Telefone, u.TipoDocumento, u.Documento, u.Endereco, u.Crc);
}
