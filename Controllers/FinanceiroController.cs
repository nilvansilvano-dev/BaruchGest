// ============================================================
// Controllers/FinanceiroController.cs
// Endpoints REST para Receita, Despesa e Resumo
// Cada dado é isolado por UsuarioId (multi-tenant).
// - perfil "usuario": sempre usa o próprio ID do JWT
// - perfil "contador": passa ?usuarioId= para ver os dados de um usuário
// ============================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Models;
using FinanceiroAPI.Repositories;
using FinanceiroAPI.Services;

namespace FinanceiroAPI.Controllers;

// Helper estático compartilhado por todos os controllers deste arquivo
internal static class UidHelper
{
    private const string SEM_USUARIO = "Como contador, informe ?usuarioId= para selecionar o usuário.";

    /// <summary>
    /// Retorna o ID efetivo a usar:
    /// - "usuario": sempre o próprio ID do JWT (ignora param)
    /// - "contador": usa o param fornecido; null se não informado
    /// </summary>
    public static (int? uid, IActionResult? erro) Resolve(ClaimsPrincipal user, int? param, ControllerBase ctrl)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.IsInRole("usuario"))
            return (int.Parse(sub!), null);

        if (param is null)
            return (null, ctrl.BadRequest(SEM_USUARIO));

        return (param, null);
    }
}

// ----------------------------------------------------------------
// ANOS FISCAIS
// ----------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/anos-fiscais")]
public class AnoFiscalController : ControllerBase
{
    private readonly IFinanceiroRepository _repo;
    public AnoFiscalController(IFinanceiroRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;

        var anos = await _repo.GetAnosFiscaisAsync(uid!.Value);
        return Ok(anos.Select(a => new AnoFiscalResponse(a.Id, a.Ano, a.Descricao, a.SaldoInicial, a.CriadoEm)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;

        var a = await _repo.GetAnoFiscalByIdAsync(id, uid!.Value);
        if (a is null) return NotFound();
        return Ok(new AnoFiscalResponse(a.Id, a.Ano, a.Descricao, a.SaldoInicial, a.CriadoEm));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AnoFiscalRequest req)
    {
        var uid = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await _repo.CreateAnoFiscalAsync(req.Ano, req.Descricao, req.SaldoInicial, uid);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}

// ----------------------------------------------------------------
// CLIENTES
// ----------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IFinanceiroRepository _repo;
    public ClienteController(IFinanceiroRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? usuarioId = null, [FromQuery] bool apenasAtivos = true)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;

        var clientes = await _repo.GetClientesAsync(uid!.Value, apenasAtivos);
        return Ok(clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.ValorMensal, c.Ativo)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;

        var c = await _repo.GetClienteByIdAsync(id, uid!.Value);
        if (c is null) return NotFound();
        return Ok(new ClienteResponse(c.Id, c.Nome, c.ValorMensal, c.Ativo));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClienteRequest req)
    {
        var uid = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await _repo.CreateClienteAsync(req.Nome, req.ValorMensal, uid);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClienteRequest req)
    {
        var uid = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repo.UpdateClienteAsync(id, req.Nome, req.ValorMensal, uid);
        return NoContent();
    }

    [HttpPatch("{id}/ativo")]
    public async Task<IActionResult> SetAtivo(int id, [FromQuery] bool ativo)
    {
        var uid = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repo.SetClienteAtivoAsync(id, ativo, uid);
        return NoContent();
    }
}

// ----------------------------------------------------------------
// RECEITA
// ----------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/receitas")]
public class ReceitaController : ControllerBase
{
    private readonly IFinanceiroRepository _repo;

    private static readonly string[] Meses =
    {
        "", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    public ReceitaController(IFinanceiroRepository repo) => _repo = repo;

    private int MyUid() => int.Parse(
        User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("grupos")]
    public async Task<IActionResult> GetGrupos([FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;
        var grupos = await _repo.GetGruposReceitaAsync(uid!.Value);
        return Ok(grupos.Select(g => new { g.Id, g.Nome, g.Ativo }));
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias([FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;
        return Ok(await _repo.GetCategoriasReceitaAsync(uid!.Value));
    }

    [HttpPost("grupos")]
    public async Task<IActionResult> CreateGrupo([FromBody] GrupoCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome)) return BadRequest("Nome obrigatório.");
        var uid = MyUid();
        var id = await _repo.CreateGrupoReceitaAsync(req.Nome.Trim(), uid);
        return Created("", new { id, req.Nome });
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CreateCategoria([FromBody] CategoriaCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome)) return BadRequest("Nome obrigatório.");
        var uid = MyUid();
        var id = await _repo.CreateCategoriaReceitaAsync(req.Nome.Trim(), req.GrupoId, uid);
        return Created("", new { id, req.Nome, req.GrupoId });
    }

    [HttpDelete("grupos/{id}")]
    public async Task<IActionResult> DeleteGrupo(int id)
    {
        await _repo.DeleteGrupoReceitaAsync(id, MyUid());
        return NoContent();
    }

    [HttpDelete("categorias/{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        await _repo.DeleteCategoriaReceitaAsync(id, MyUid());
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int anoFiscalId,
        [FromQuery] int? mes = null,
        [FromQuery] int? clienteId = null)
    {
        var lancamentos = await _repo.GetLancamentosReceitaAsync(anoFiscalId, mes, clienteId);

        var response = lancamentos.Select(l => new LancamentoReceitaResponse(
            l.Id, l.AnoFiscal!.Ano,
            l.Categoria!.Grupo!.Nome, l.Categoria!.Nome,
            l.Cliente?.Nome, l.Mes, Meses[l.Mes],
            l.Valor, l.Observacao, l.LancadoEm
        ));

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LancamentoReceitaRequest req)
    {
        if (req.Mes is < 1 or > 12)
            return BadRequest("Mês deve estar entre 1 e 12.");

        var lanc = new LancamentoReceita
        {
            AnoFiscalId        = req.AnoFiscalId,
            CategoriaReceitaId = req.CategoriaReceitaId,
            ClienteId          = req.ClienteId,
            Mes                = req.Mes,
            Valor              = req.Valor,
            Observacao         = req.Observacao
        };

        var id = await _repo.CreateLancamentoReceitaAsync(lanc);
        return CreatedAtAction(nameof(GetAll), new { anoFiscalId = req.AnoFiscalId }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LancamentoUpdateRequest req)
    {
        await _repo.UpdateLancamentoReceitaAsync(id, req.Valor, req.Observacao);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteLancamentoReceitaAsync(id);
        return NoContent();
    }
}

// ----------------------------------------------------------------
// DESPESA
// ----------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/despesas")]
public class DespesaController : ControllerBase
{
    private readonly IFinanceiroRepository _repo;

    private static readonly string[] Meses =
    {
        "", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    public DespesaController(IFinanceiroRepository repo) => _repo = repo;

    private int MyUid() => int.Parse(
        User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("grupos")]
    public async Task<IActionResult> GetGrupos([FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;
        var grupos = await _repo.GetGruposDespesaAsync(uid!.Value);
        return Ok(grupos.Select(g => new { g.Id, g.Nome, g.Ativo }));
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias([FromQuery] int? usuarioId = null, [FromQuery] int? grupoId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;
        return Ok(await _repo.GetCategoriasDespesaAsync(uid!.Value, grupoId));
    }

    [HttpPost("grupos")]
    public async Task<IActionResult> CreateGrupo([FromBody] GrupoCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome)) return BadRequest("Nome obrigatório.");
        var uid = MyUid();
        var id = await _repo.CreateGrupoDespesaAsync(req.Nome.Trim(), uid);
        return Created("", new { id, req.Nome });
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CreateCategoria([FromBody] CategoriaCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome)) return BadRequest("Nome obrigatório.");
        var uid = MyUid();
        var id = await _repo.CreateCategoriaDespesaAsync(req.Nome.Trim(), req.GrupoId, uid);
        return Created("", new { id, req.Nome, req.GrupoId });
    }

    [HttpDelete("grupos/{id}")]
    public async Task<IActionResult> DeleteGrupo(int id)
    {
        await _repo.DeleteGrupoDespesaAsync(id, MyUid());
        return NoContent();
    }

    [HttpDelete("categorias/{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        await _repo.DeleteCategoriaDespesaAsync(id, MyUid());
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int anoFiscalId,
        [FromQuery] int? mes = null,
        [FromQuery] int? grupoId = null)
    {
        var lancamentos = await _repo.GetLancamentosDespesaAsync(anoFiscalId, mes, grupoId);

        var response = lancamentos.Select(l => new LancamentoDespesaResponse(
            l.Id, l.AnoFiscal!.Ano,
            l.Categoria!.Grupo!.Nome, l.Categoria!.Nome,
            l.Mes, Meses[l.Mes],
            l.Valor, l.Observacao, l.LancadoEm
        ));

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LancamentoDespesaRequest req)
    {
        if (req.Mes is < 1 or > 12)
            return BadRequest("Mês deve estar entre 1 e 12.");

        var lanc = new LancamentoDespesa
        {
            AnoFiscalId        = req.AnoFiscalId,
            CategoriaDespesaId = req.CategoriaDespesaId,
            Mes                = req.Mes,
            Valor              = req.Valor,
            Observacao         = req.Observacao
        };

        var id = await _repo.CreateLancamentoDespesaAsync(lanc);
        return CreatedAtAction(nameof(GetAll), new { anoFiscalId = req.AnoFiscalId }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LancamentoUpdateRequest req)
    {
        await _repo.UpdateLancamentoDespesaAsync(id, req.Valor, req.Observacao);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteLancamentoDespesaAsync(id);
        return NoContent();
    }
}

// ----------------------------------------------------------------
// RESUMO
// ----------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/resumo")]
public class ResumoController : ControllerBase
{
    private readonly ResumoService _resumoService;

    public ResumoController(ResumoService resumoService) => _resumoService = resumoService;

    [HttpGet("{ano}")]
    public async Task<IActionResult> GetResumoAnual(int ano, [FromQuery] int? usuarioId = null)
    {
        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;
        return Ok(await _resumoService.GetResumoAnualAsync(ano, uid!.Value));
    }

    [HttpGet("{ano}/{mes}")]
    public async Task<IActionResult> GetResumoMes(int ano, int mes, [FromQuery] int? usuarioId = null)
    {
        if (mes is < 1 or > 12)
            return BadRequest("Mês deve estar entre 1 e 12.");

        var (uid, erro) = UidHelper.Resolve(User, usuarioId, this);
        if (erro is not null) return erro;

        var resultado = await _resumoService.GetResumoMesAsync(ano, mes, uid!.Value);
        if (resultado is null)
            return NotFound("Nenhum lançamento encontrado para este período.");

        return Ok(resultado);
    }
}

// DTO compartilhado para updates
public record LancamentoUpdateRequest(decimal Valor, string? Observacao);
