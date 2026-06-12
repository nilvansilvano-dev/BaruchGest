// ============================================================
// Repositories/FinanceiroRepository.cs
// Escrita: EF Core (via FinanceiroDbContext)
// Leitura complexa: Dapper (queries com JOIN/GROUP BY e views)
// ============================================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Data;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Repositories;

public interface IFinanceiroRepository
{
    // Ano Fiscal
    Task<IEnumerable<AnoFiscal>> GetAnosFiscaisAsync();
    Task<AnoFiscal?> GetAnoFiscalByIdAsync(int id);
    Task<int> CreateAnoFiscalAsync(int ano, string? descricao, decimal saldoInicial = 0);

    // Clientes
    Task<IEnumerable<Cliente>> GetClientesAsync(bool apenasAtivos = true);
    Task<Cliente?> GetClienteByIdAsync(int id);
    Task<int> CreateClienteAsync(string nome, decimal? valorMensal);
    Task UpdateClienteAsync(int id, string nome, decimal? valorMensal);
    Task SetClienteAtivoAsync(int id, bool ativo);

    // Receita
    Task<IEnumerable<CategoriaReceita>> GetCategoriasReceitaAsync();
    Task<IEnumerable<LancamentoReceita>> GetLancamentosReceitaAsync(int anoFiscalId, int? mes = null, int? clienteId = null);
    Task<int> CreateLancamentoReceitaAsync(LancamentoReceita lanc);
    Task UpdateLancamentoReceitaAsync(int id, decimal valor, string? observacao);
    Task DeleteLancamentoReceitaAsync(int id);

    // Despesa
    Task<IEnumerable<GrupoDespesa>> GetGruposDespesaAsync();
    Task<IEnumerable<CategoriaDespesa>> GetCategoriasDespesaAsync(int? grupoId = null);
    Task<IEnumerable<LancamentoDespesa>> GetLancamentosDespesaAsync(int anoFiscalId, int? mes = null, int? grupoId = null);
    Task<int> CreateLancamentoDespesaAsync(LancamentoDespesa lanc);
    Task UpdateLancamentoDespesaAsync(int id, decimal valor, string? observacao);
    Task DeleteLancamentoDespesaAsync(int id);

    // Resumo
    Task<IEnumerable<ResumoMensal>> GetResumoMensalAsync(int ano);
    Task<IEnumerable<DespesaPorGrupo>> GetDespesaPorGrupoAsync(int ano, int? mes = null);
}

public class FinanceiroRepository : IFinanceiroRepository
{
    private readonly FinanceiroDbContext _db;
    private readonly string _connectionString;

    public FinanceiroRepository(FinanceiroDbContext db, IConfiguration config)
    {
        _db = db;
        _connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' não encontrada.");
    }

    private IDbConnection Dapper => new SqlConnection(_connectionString);

    // -------- ANO FISCAL (EF Core) --------

    public async Task<IEnumerable<AnoFiscal>> GetAnosFiscaisAsync() =>
        await _db.AnosFiscais.OrderByDescending(x => x.Ano).ToListAsync();

    public async Task<AnoFiscal?> GetAnoFiscalByIdAsync(int id) =>
        await _db.AnosFiscais.FindAsync(id);

    public async Task<int> CreateAnoFiscalAsync(int ano, string? descricao, decimal saldoInicial = 0)
    {
        var entity = new AnoFiscal { Ano = ano, Descricao = descricao, SaldoInicial = saldoInicial };
        _db.AnosFiscais.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    // -------- CLIENTES (EF Core) --------

    public async Task<IEnumerable<Cliente>> GetClientesAsync(bool apenasAtivos = true) =>
        await _db.Clientes
            .Where(c => !apenasAtivos || c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public async Task<Cliente?> GetClienteByIdAsync(int id) =>
        await _db.Clientes.FindAsync(id);

    public async Task<int> CreateClienteAsync(string nome, decimal? valorMensal)
    {
        var entity = new Cliente { Nome = nome, ValorMensal = valorMensal };
        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateClienteAsync(int id, string nome, decimal? valorMensal)
    {
        var entity = await _db.Clientes.FindAsync(id);
        if (entity is null) return;
        entity.Nome = nome;
        entity.ValorMensal = valorMensal;
        await _db.SaveChangesAsync();
    }

    public async Task SetClienteAtivoAsync(int id, bool ativo)
    {
        var entity = await _db.Clientes.FindAsync(id);
        if (entity is null) return;
        entity.Ativo = ativo;
        await _db.SaveChangesAsync();
    }

    // -------- RECEITA — leitura (Dapper) --------

    public async Task<IEnumerable<CategoriaReceita>> GetCategoriasReceitaAsync()
    {
        using var db = Dapper;
        var sql = @"
            SELECT cr.*, gr.Id, gr.Nome, gr.Ativo
            FROM CategoriaReceita cr
            INNER JOIN GrupoReceita gr ON gr.Id = cr.GrupoReceitaId
            WHERE cr.Ativo = 1
            ORDER BY gr.Nome, cr.Nome";

        return await db.QueryAsync<CategoriaReceita, GrupoReceita, CategoriaReceita>(
            sql,
            (cat, grp) => { cat.Grupo = grp; return cat; },
            splitOn: "Id");
    }

    public async Task<IEnumerable<LancamentoReceita>> GetLancamentosReceitaAsync(
        int anoFiscalId, int? mes = null, int? clienteId = null)
    {
        using var db = Dapper;
        var sql = @"
            SELECT lr.*, cr.Id, cr.Nome, cr.GrupoReceitaId, cr.Ativo,
                   gr.Id, gr.Nome, gr.Ativo,
                   c.Id, c.Nome, c.ValorMensal, c.Ativo, c.CriadoEm,
                   af.Id, af.Ano, af.Descricao, af.CriadoEm
            FROM LancamentoReceita lr
            INNER JOIN CategoriaReceita cr ON cr.Id = lr.CategoriaReceitaId
            INNER JOIN GrupoReceita gr     ON gr.Id = cr.GrupoReceitaId
            LEFT  JOIN Cliente c           ON c.Id  = lr.ClienteId
            INNER JOIN AnoFiscal af        ON af.Id = lr.AnoFiscalId
            WHERE lr.AnoFiscalId = @AnoFiscalId
              AND (@Mes IS NULL OR lr.Mes = @Mes)
              AND (@ClienteId IS NULL OR lr.ClienteId = @ClienteId)
            ORDER BY lr.Mes, cr.Nome";

        return await db.QueryAsync<LancamentoReceita, CategoriaReceita, GrupoReceita, Cliente, AnoFiscal, LancamentoReceita>(
            sql,
            (lanc, cat, grp, cli, af) =>
            {
                cat.Grupo = grp;
                lanc.Categoria = cat;
                lanc.Cliente = cli;
                lanc.AnoFiscal = af;
                return lanc;
            },
            new { AnoFiscalId = anoFiscalId, Mes = mes, ClienteId = clienteId },
            splitOn: "Id,Id,Id,Id");
    }

    // -------- RECEITA — escrita (EF Core) --------

    public async Task<int> CreateLancamentoReceitaAsync(LancamentoReceita lanc)
    {
        _db.LancamentosReceita.Add(lanc);
        await _db.SaveChangesAsync();
        return lanc.Id;
    }

    public async Task UpdateLancamentoReceitaAsync(int id, decimal valor, string? observacao)
    {
        var entity = await _db.LancamentosReceita.FindAsync(id);
        if (entity is null) return;
        entity.Valor = valor;
        entity.Observacao = observacao;
        entity.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLancamentoReceitaAsync(int id)
    {
        var entity = await _db.LancamentosReceita.FindAsync(id);
        if (entity is null) return;
        _db.LancamentosReceita.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // -------- DESPESA — leitura (Dapper) --------

    public async Task<IEnumerable<GrupoDespesa>> GetGruposDespesaAsync()
    {
        using var db = Dapper;
        return await db.QueryAsync<GrupoDespesa>(
            "SELECT * FROM GrupoDespesa WHERE Ativo = 1 ORDER BY Nome");
    }

    public async Task<IEnumerable<CategoriaDespesa>> GetCategoriasDespesaAsync(int? grupoId = null)
    {
        using var db = Dapper;
        var sql = @"
            SELECT cd.*, gd.Id, gd.Nome, gd.Ativo
            FROM CategoriaDespesa cd
            INNER JOIN GrupoDespesa gd ON gd.Id = cd.GrupoDespesaId
            WHERE cd.Ativo = 1
              AND (@GrupoId IS NULL OR cd.GrupoDespesaId = @GrupoId)
            ORDER BY gd.Nome, cd.Nome";

        return await db.QueryAsync<CategoriaDespesa, GrupoDespesa, CategoriaDespesa>(
            sql,
            (cat, grp) => { cat.Grupo = grp; return cat; },
            new { GrupoId = grupoId },
            splitOn: "Id");
    }

    public async Task<IEnumerable<LancamentoDespesa>> GetLancamentosDespesaAsync(
        int anoFiscalId, int? mes = null, int? grupoId = null)
    {
        using var db = Dapper;
        var sql = @"
            SELECT ld.*, cd.Id, cd.Nome, cd.GrupoDespesaId, cd.Ativo,
                   gd.Id, gd.Nome, gd.Ativo,
                   af.Id, af.Ano, af.Descricao, af.CriadoEm
            FROM LancamentoDespesa ld
            INNER JOIN CategoriaDespesa cd ON cd.Id = ld.CategoriaDespesaId
            INNER JOIN GrupoDespesa gd     ON gd.Id = cd.GrupoDespesaId
            INNER JOIN AnoFiscal af        ON af.Id = ld.AnoFiscalId
            WHERE ld.AnoFiscalId = @AnoFiscalId
              AND (@Mes IS NULL OR ld.Mes = @Mes)
              AND (@GrupoId IS NULL OR cd.GrupoDespesaId = @GrupoId)
            ORDER BY ld.Mes, gd.Nome, cd.Nome";

        return await db.QueryAsync<LancamentoDespesa, CategoriaDespesa, GrupoDespesa, AnoFiscal, LancamentoDespesa>(
            sql,
            (lanc, cat, grp, af) =>
            {
                cat.Grupo = grp;
                lanc.Categoria = cat;
                lanc.AnoFiscal = af;
                return lanc;
            },
            new { AnoFiscalId = anoFiscalId, Mes = mes, GrupoId = grupoId },
            splitOn: "Id,Id,Id");
    }

    // -------- DESPESA — escrita (EF Core) --------

    public async Task<int> CreateLancamentoDespesaAsync(LancamentoDespesa lanc)
    {
        _db.LancamentosDespesa.Add(lanc);
        await _db.SaveChangesAsync();
        return lanc.Id;
    }

    public async Task UpdateLancamentoDespesaAsync(int id, decimal valor, string? observacao)
    {
        var entity = await _db.LancamentosDespesa.FindAsync(id);
        if (entity is null) return;
        entity.Valor = valor;
        entity.Observacao = observacao;
        entity.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLancamentoDespesaAsync(int id)
    {
        var entity = await _db.LancamentosDespesa.FindAsync(id);
        if (entity is null) return;
        _db.LancamentosDespesa.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // -------- RESUMO (Dapper — views) --------

    public async Task<IEnumerable<ResumoMensal>> GetResumoMensalAsync(int ano)
    {
        using var db = Dapper;
        return await db.QueryAsync<ResumoMensal>(
            "SELECT * FROM vw_ResumoMensal WHERE Ano = @Ano ORDER BY Mes",
            new { Ano = ano });
    }

    public async Task<IEnumerable<DespesaPorGrupo>> GetDespesaPorGrupoAsync(int ano, int? mes = null)
    {
        using var db = Dapper;
        return await db.QueryAsync<DespesaPorGrupo>(
            "SELECT * FROM vw_DespesaPorGrupo WHERE Ano = @Ano AND (@Mes IS NULL OR Mes = @Mes) ORDER BY Total DESC",
            new { Ano = ano, Mes = mes });
    }
}
