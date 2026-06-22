using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Controllers;

[ApiController]
[Route("api/convites")]
public class ConviteController : ControllerBase
{
    private readonly FinanceiroDbContext _db;

    public ConviteController(FinanceiroDbContext db) => _db = db;

    private int GetContadorId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(sub!);
    }

    private ConviteResponse ToResponse(Convite c, string baseUrl) =>
        new(c.Id, c.Token, c.EmailConvidado, c.Usado, c.CriadoEm, c.ExpiraEm,
            $"{baseUrl}/login?convite={c.Token}");

    private string BaseUrl()
    {
        // URL do frontend (Vite dev)
        var origin = Request.Headers["Origin"].ToString();
        return string.IsNullOrEmpty(origin) ? "http://localhost:5173" : origin;
    }

    /// <summary>Gera um novo convite de cadastro. Apenas o contador pode gerar.</summary>
    [Authorize(Roles = "contador")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ConviteCreateRequest req)
    {
        if (req.DiasValidade is < 1 or > 30)
            return BadRequest("Validade deve ser entre 1 e 30 dias.");

        var contadorId = GetContadorId();
        var convite = new Convite
        {
            Token          = Guid.NewGuid().ToString(),
            ContadorId     = contadorId,
            EmailConvidado = req.EmailConvidado?.Trim().ToLowerInvariant(),
            ExpiraEm       = DateTime.UtcNow.AddDays(req.DiasValidade)
        };

        _db.Convites.Add(convite);
        await _db.SaveChangesAsync();

        return Created("", ToResponse(convite, BaseUrl()));
    }

    /// <summary>Lista os convites gerados por este contador.</summary>
    [Authorize(Roles = "contador")]
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var contadorId = GetContadorId();
        var convites = await _db.Convites
            .Where(c => c.ContadorId == contadorId)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync();

        var baseUrl = BaseUrl();
        return Ok(convites.Select(c => ToResponse(c, baseUrl)));
    }

    /// <summary>Valida um token de convite. Endpoint público — usado na tela de cadastro.</summary>
    [AllowAnonymous]
    [HttpGet("validar/{token}")]
    public async Task<IActionResult> Validar(string token)
    {
        var convite = await _db.Convites.FirstOrDefaultAsync(c => c.Token == token);

        if (convite is null)
            return Ok(new ConviteValidarResponse(false, null, "Convite não encontrado."));

        if (convite.Usado)
            return Ok(new ConviteValidarResponse(false, null, "Este convite já foi utilizado."));

        if (convite.ExpiraEm < DateTime.UtcNow)
            return Ok(new ConviteValidarResponse(false, null, "Este convite está expirado."));

        return Ok(new ConviteValidarResponse(true, convite.EmailConvidado, null));
    }

    /// <summary>Marca um convite como usado. Chamado internamente após registro.</summary>
    [AllowAnonymous]
    [HttpPost("usar/{token}")]
    public async Task<IActionResult> Usar(string token)
    {
        var convite = await _db.Convites.FirstOrDefaultAsync(c => c.Token == token);
        if (convite is null || convite.Usado || convite.ExpiraEm < DateTime.UtcNow)
            return BadRequest("Convite inválido.");

        convite.Usado = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
