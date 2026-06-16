// ============================================================
// Controllers/FinanceiroController.cs
// Endpoints REST para Receita, Despesa e Resumo
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceiroAPI.DTOs;
using FinanceiroAPI.Models;
using FinanceiroAPI.Repositories;
using FinanceiroAPI.Services;

namespace FinanceiroAPI.Controllers;

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
    public async Task<IActionResult> GetAll()
    {
        var anos = await _repo.GetAnosFiscaisAsync();
        return Ok(anos.Select(a => new AnoFiscalResponse(a.Id, a.Ano, a.Descricao, a.SaldoInicial, a.CriadoEm)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _repo.GetAnoFiscalByIdAsync(id);
        if (a is null) return NotFound();
        return Ok(new AnoFiscalResponse(a.Id, a.Ano, a.Descricao, a.SaldoInicial, a.CriadoEm));
    }

    [Authorize(Roles = "usuario")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AnoFiscalRequest req)
    {
        var id = await _repo.CreateAnoFiscalAsync(req.Ano, req.Descricao, req.SaldoInicial);
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

    /// <summary>Lista clientes. ?apenasAtivos=false para incluir inativos.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool apenasAtivos = true)
    {
        var clientes = await _repo.GetClientesAsync(apenasAtivos);
        return Ok(clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.ValorMensal, c.Ativo)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _repo.GetClienteByIdAsync(id);
        if (c is null) return NotFound();
        return Ok(new ClienteResponse(c.Id, c.Nome, c.ValorMensal, c.Ativo));
    }

    [Authorize(Roles = "usuario")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClienteRequest req)
    {
        var id = await _repo.CreateClienteAsync(req.Nome, req.ValorMensal);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [Authorize(Roles = "usuario")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClienteRequest req)
    {
        await _repo.UpdateClienteAsync(id, req.Nome, req.ValorMensal);
        return NoContent();
    }

    [Authorize(Roles = "usuario")]
    [HttpPatch("{id}/ativo")]
    public async Task<IActionResult> SetAtivo(int id, [FromQuery] bool ativo)
    {
        await _repo.SetClienteAtivoAsync(id, ativo);
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

    /// <summary>Lista categorias de receita disponíveis.</summary>
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias() =>
        Ok(await _repo.GetCategoriasReceitaAsync());

    /// <summary>
    /// Lista lançamentos de receita.
    /// Query params: anoFiscalId (obrigatório), mes (opcional), clienteId (opcional)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int anoFiscalId,
        [FromQuery] int? mes = null,
        [FromQuery] int? clienteId = null)
    {
        var lancamentos = await _repo.GetLancamentosReceitaAsync(anoFiscalId, mes, clienteId);

        var response = lancamentos.Select(l => new LancamentoReceitaResponse(
            l.Id,
            l.AnoFiscal!.Ano,
            l.Categoria!.Grupo!.Nome,
            l.Categoria!.Nome,
            l.Cliente?.Nome,
            l.Mes,
            Meses[l.Mes],
            l.Valor,
            l.Observacao,
            l.LancadoEm
        ));

        return Ok(response);
    }

    [Authorize(Roles = "usuario")]
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

    [Authorize(Roles = "usuario")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LancamentoUpdateRequest req)
    {
        await _repo.UpdateLancamentoReceitaAsync(id, req.Valor, req.Observacao);
        return NoContent();
    }

    [Authorize(Roles = "usuario")]
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

    /// <summary>Lista grupos de despesa.</summary>
    [HttpGet("grupos")]
    public async Task<IActionResult> GetGrupos() =>
        Ok(await _repo.GetGruposDespesaAsync());

    /// <summary>Lista categorias. ?grupoId= para filtrar por grupo.</summary>
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias([FromQuery] int? grupoId = null) =>
        Ok(await _repo.GetCategoriasDespesaAsync(grupoId));

    /// <summary>
    /// Lista lançamentos de despesa.
    /// Query params: anoFiscalId (obrigatório), mes (opcional), grupoId (opcional)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int anoFiscalId,
        [FromQuery] int? mes = null,
        [FromQuery] int? grupoId = null)
    {
        var lancamentos = await _repo.GetLancamentosDespesaAsync(anoFiscalId, mes, grupoId);

        var response = lancamentos.Select(l => new LancamentoDespesaResponse(
            l.Id,
            l.AnoFiscal!.Ano,
            l.Categoria!.Grupo!.Nome,
            l.Categoria!.Nome,
            l.Mes,
            Meses[l.Mes],
            l.Valor,
            l.Observacao,
            l.LancadoEm
        ));

        return Ok(response);
    }

    [Authorize(Roles = "usuario")]
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

    [Authorize(Roles = "usuario")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LancamentoUpdateRequest req)
    {
        await _repo.UpdateLancamentoDespesaAsync(id, req.Valor, req.Observacao);
        return NoContent();
    }

    [Authorize(Roles = "usuario")]
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

    /// <summary>Resumo anual completo. SaldoAcumulado parte do SaldoInicial do AnoFiscal.</summary>
    [HttpGet("{ano}")]
    public async Task<IActionResult> GetResumoAnual(int ano) =>
        Ok(await _resumoService.GetResumoAnualAsync(ano));

    /// <summary>Resumo de um mês específico com SaldoAcumulado até o mês.</summary>
    [HttpGet("{ano}/{mes}")]
    public async Task<IActionResult> GetResumoMes(int ano, int mes)
    {
        if (mes is < 1 or > 12)
            return BadRequest("Mês deve estar entre 1 e 12.");

        var resultado = await _resumoService.GetResumoMesAsync(ano, mes);
        if (resultado is null)
            return NotFound("Nenhum lançamento encontrado para este período.");

        return Ok(resultado);
    }
}

// DTO compartilhado para updates
public record LancamentoUpdateRequest(decimal Valor, string? Observacao);
