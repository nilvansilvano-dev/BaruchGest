using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Controllers;

[Authorize(Roles = "contador")]
[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly FinanceiroDbContext _db;

    public UsuarioController(FinanceiroDbContext db) => _db = db;

    private static UsuarioResponse ToResponse(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Ativo, u.CriadoEm,
            u.Telefone, u.TipoDocumento, u.Documento, u.Endereco, u.Crc);

    /// <summary>Lista todos os usuários (perfil cliente/usuario). Apenas o contador pode listar.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _db.Usuarios
            .Where(u => u.Perfil == "usuario")
            .OrderBy(u => u.Nome).ThenBy(u => u.Email)
            .ToListAsync();

        return Ok(usuarios.Select(ToResponse));
    }

    /// <summary>Cria novo usuário com perfil 'usuario'. Apenas o contador pode criar.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UsuarioCreateRequest req)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email já cadastrado.");

        var usuario = new Usuario
        {
            Nome      = req.Nome,
            Email     = req.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.Senha),
            Perfil    = "usuario",
            Ativo     = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = usuario.Id }, ToResponse(usuario));
    }

    /// <summary>Ativa ou desativa um usuário. Apenas o contador pode fazer isso.</summary>
    [HttpPatch("{id}/ativo")]
    public async Task<IActionResult> SetAtivo(int id, [FromQuery] bool ativo)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();
        if (usuario.Perfil == "contador") return BadRequest("Não é possível desativar o contador.");

        usuario.Ativo = ativo;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
