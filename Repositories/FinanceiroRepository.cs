// ============================================================
// Repositories/FinanceiroRepository.cs
// Escrita: EF Core (via FinanceiroDbContext)
// Leitura complexa: Dapper (queries com JOIN/GROUP BY)
// Todos os dados são isolados por UsuarioId (multi-tenant por usuário)
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
    Task<IEnumerable<AnoFiscal>> GetAnosFiscaisAsync(int usuarioId);
    Task<AnoFiscal?> GetAnoFiscalByIdAsync(int id, int usuarioId);
    Task<int> CreateAnoFiscalAsync(int ano, string? descricao, decimal saldoInicial, int usuarioId);

    // Clientes
    Task<IEnumerable<Cliente>> GetClientesAsync(int usuarioId, bool apenasAtivos = true);
    Task<Cliente?> GetClienteByIdAsync(int id, int usuarioId);
    Task<int> CreateClienteAsync(string nome, decimal? valorMensal, int usuarioId);
    Task UpdateClienteAsync(int id, string nome, decimal? valorMensal, int usuarioId);
    Task SetClienteAtivoAsync(int id, bool ativo, int usuarioId);

    // Plano de Contas — Receita
    Task<IEnumerable<GrupoReceita>> GetGruposReceitaAsync(int usuarioId);
    Task<IEnumerable<CategoriaReceita>> GetCategoriasReceitaAsync(int usuarioId);
    Task<int> CreateGrupoReceitaAsync(string nome, int usuarioId);
    Task<int> CreateCategoriaReceitaAsync(string nome, int grupoId, int usuarioId);
    Task DeleteGrupoReceitaAsync(int id, int usuarioId);
    Task DeleteCategoriaReceitaAsync(int id, int usuarioId);

    // Lancamentos Receita
    Task<IEnumerable<LancamentoReceita>> GetLancamentosReceitaAsync(int anoFiscalId, int? mes = null, int? clienteId = null);
    Task<int> CreateLancamentoReceitaAsync(LancamentoReceita lanc);
    Task UpdateLancamentoReceitaAsync(int id, decimal valor, string? observacao);
    Task DeleteLancamentoReceitaAsync(int id);

    // Plano de Contas — Despesa
    Task<IEnumerable<GrupoDespesa>> GetGruposDespesaAsync(int usuarioId);
    Task<IEnumerable<CategoriaDespesa>> GetCategoriasDespesaAsync(int usuarioId, int? grupoId = null);
    Task<int> CreateGrupoDespesaAsync(string nome, int usuarioId);
    Task<int> CreateCategoriaDespesaAsync(string nome, int grupoId, int usuarioId);
    Task DeleteGrupoDespesaAsync(int id, int usuarioId);
    Task DeleteCategoriaDespesaAsync(int id, int usuarioId);

    // Lancamentos Despesa
    Task<IEnumerable<LancamentoDespesa>> GetLancamentosDespesaAsync(int anoFiscalId, int? mes = null, int? grupoId = null);
    Task<int> CreateLancamentoDespesaAsync(LancamentoDespesa lanc);
    Task UpdateLancamentoDespesaAsync(int id, decimal valor, string? observacao);
    Task DeleteLancamentoDespesaAsync(int id);

    // Resumo (direto, sem views — filtra por usuarioId via AnoFiscal)
    Task<IEnumerable<ResumoMensal>> GetResumoMensalAsync(int ano, int usuarioId);
    Task<IEnumerable<DespesaPorGrupo>> GetDespesaPorGrupoAsync(int ano, int usuarioId, int? mes = null);
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

    public async Task<IEnumerable<AnoFiscal>> GetAnosFiscaisAsync(int usuarioId) =>
        await _db.AnosFiscais
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.Ano)
            .ToListAsync();

    public async Task<AnoFiscal?> GetAnoFiscalByIdAsync(int id, int usuarioId) =>
        await _db.AnosFiscais.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId);

    public async Task<int> CreateAnoFiscalAsync(int ano, string? descricao, decimal saldoInicial, int usuarioId)
    {
        var entity = new AnoFiscal { Ano = ano, Descricao = descricao, SaldoInicial = saldoInicial, UsuarioId = usuarioId };
        _db.AnosFiscais.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    // -------- CLIENTES (EF Core) --------

    public async Task<IEnumerable<Cliente>> GetClientesAsync(int usuarioId, bool apenasAtivos = true) =>
        await _db.Clientes
            .Where(c => c.UsuarioId == usuarioId && (!apenasAtivos || c.Ativo))
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public async Task<Cliente?> GetClienteByIdAsync(int id, int usuarioId) =>
        await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);

    public async Task<int> CreateClienteAsync(string nome, decimal? valorMensal, int usuarioId)
    {
        var entity = new Cliente { Nome = nome, ValorMensal = valorMensal, UsuarioId = usuarioId };
        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateClienteAsync(int id, string nome, decimal? valorMensal, int usuarioId)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Nome = nome;
        entity.ValorMensal = valorMensal;
        await _db.SaveChangesAsync();
    }

    public async Task SetClienteAtivoAsync(int id, bool ativo, int usuarioId)
    {
        var entity = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Ativo = ativo;
        await _db.SaveChangesAsync();
    }

    // -------- PLANO DE CONTAS — RECEITA (EF Core) --------

    public async Task<IEnumerable<GrupoReceita>> GetGruposReceitaAsync(int usuarioId) =>
        await _db.Set<GrupoReceita>()
            .Where(g => g.UsuarioId == usuarioId && g.Ativo)
            .OrderBy(g => g.Nome)
            .ToListAsync();

    public async Task<IEnumerable<CategoriaReceita>> GetCategoriasReceitaAsync(int usuarioId)
    {
        using var db = Dapper;
        var sql = @"
            SELECT cr.*, gr.Id, gr.Nome, gr.Ativo
            FROM CategoriaReceita cr
            INNER JOIN GrupoReceita gr ON gr.Id = cr.GrupoReceitaId
            WHERE cr.Ativo = 1 AND cr.UsuarioId = @UsuarioId
            ORDER BY gr.Nome, cr.Nome";

        return await db.QueryAsync<CategoriaReceita, GrupoReceita, CategoriaReceita>(
            sql,
            (cat, grp) => { cat.Grupo = grp; return cat; },
            new { UsuarioId = usuarioId },
            splitOn: "Id");
    }

    public async Task<int> CreateGrupoReceitaAsync(string nome, int usuarioId)
    {
        var entity = new GrupoReceita { Nome = nome, UsuarioId = usuarioId };
        _db.Set<GrupoReceita>().Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<int> CreateCategoriaReceitaAsync(string nome, int grupoId, int usuarioId)
    {
        var entity = new CategoriaReceita { Nome = nome, GrupoReceitaId = grupoId, UsuarioId = usuarioId };
        _db.Set<CategoriaReceita>().Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteGrupoReceitaAsync(int id, int usuarioId)
    {
        var entity = await _db.Set<GrupoReceita>().FirstOrDefaultAsync(g => g.Id == id && g.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Ativo = false;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCategoriaReceitaAsync(int id, int usuarioId)
    {
        var entity = await _db.Set<CategoriaReceita>().FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Ativo = false;
        await _db.SaveChangesAsync();
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

    // -------- PLANO DE CONTAS — DESPESA (EF Core) --------

    public async Task<IEnumerable<GrupoDespesa>> GetGruposDespesaAsync(int usuarioId) =>
        await _db.Set<GrupoDespesa>()
            .Where(g => g.UsuarioId == usuarioId && g.Ativo)
            .OrderBy(g => g.Nome)
            .ToListAsync();

    public async Task<IEnumerable<CategoriaDespesa>> GetCategoriasDespesaAsync(int usuarioId, int? grupoId = null)
    {
        using var db = Dapper;
        var sql = @"
            SELECT cd.*, gd.Id, gd.Nome, gd.Ativo
            FROM CategoriaDespesa cd
            INNER JOIN GrupoDespesa gd ON gd.Id = cd.GrupoDespesaId
            WHERE cd.Ativo = 1 AND cd.UsuarioId = @UsuarioId
              AND (@GrupoId IS NULL OR cd.GrupoDespesaId = @GrupoId)
            ORDER BY gd.Nome, cd.Nome";

        return await db.QueryAsync<CategoriaDespesa, GrupoDespesa, CategoriaDespesa>(
            sql,
            (cat, grp) => { cat.Grupo = grp; return cat; },
            new { UsuarioId = usuarioId, GrupoId = grupoId },
            splitOn: "Id");
    }

    public async Task<int> CreateGrupoDespesaAsync(string nome, int usuarioId)
    {
        var entity = new GrupoDespesa { Nome = nome, UsuarioId = usuarioId };
        _db.Set<GrupoDespesa>().Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<int> CreateCategoriaDespesaAsync(string nome, int grupoId, int usuarioId)
    {
        var entity = new CategoriaDespesa { Nome = nome, GrupoDespesaId = grupoId, UsuarioId = usuarioId };
        _db.Set<CategoriaDespesa>().Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteGrupoDespesaAsync(int id, int usuarioId)
    {
        var entity = await _db.Set<GrupoDespesa>().FirstOrDefaultAsync(g => g.Id == id && g.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Ativo = false;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCategoriaDespesaAsync(int id, int usuarioId)
    {
        var entity = await _db.Set<CategoriaDespesa>().FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
        if (entity is null) return;
        entity.Ativo = false;
        await _db.SaveChangesAsync();
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

    // -------- RESUMO (Dapper — queries diretas filtrando por UsuarioId) --------

    public async Task<IEnumerable<ResumoMensal>> GetResumoMensalAsync(int ano, int usuarioId)
    {
        using var db = Dapper;
        var sql = @"
            SELECT
                @Ano AS Ano,
                m.Mes,
                ISNULL(r.TotalReceita, 0) AS TotalReceita,
                ISNULL(d.TotalDespesa, 0) AS TotalDespesa,
                ISNULL(r.TotalReceita, 0) - ISNULL(d.TotalDespesa, 0) AS SaldoMensal
            FROM (
                SELECT 1 AS Mes UNION SELECT 2 UNION SELECT 3  UNION SELECT 4
                UNION SELECT 5  UNION SELECT 6 UNION SELECT 7  UNION SELECT 8
                UNION SELECT 9  UNION SELECT 10 UNION SELECT 11 UNION SELECT 12
            ) m
            LEFT JOIN (
                SELECT lr.Mes, SUM(lr.Valor) AS TotalReceita
                FROM LancamentoReceita lr
                INNER JOIN AnoFiscal af ON af.Id = lr.AnoFiscalId
                    AND af.Ano = @Ano AND af.UsuarioId = @UsuarioId
                GROUP BY lr.Mes
            ) r ON r.Mes = m.Mes
            LEFT JOIN (
                SELECT ld.Mes, SUM(ld.Valor) AS TotalDespesa
                FROM LancamentoDespesa ld
                INNER JOIN AnoFiscal af ON af.Id = ld.AnoFiscalId
                    AND af.Ano = @Ano AND af.UsuarioId = @UsuarioId
                GROUP BY ld.Mes
            ) d ON d.Mes = m.Mes
            WHERE r.TotalReceita IS NOT NULL OR d.TotalDespesa IS NOT NULL
            ORDER BY m.Mes";

        return await db.QueryAsync<ResumoMensal>(sql, new { Ano = ano, UsuarioId = usuarioId });
    }

    public async Task<IEnumerable<DespesaPorGrupo>> GetDespesaPorGrupoAsync(int ano, int usuarioId, int? mes = null)
    {
        using var db = Dapper;
        var sql = @"
            SELECT
                @Ano AS Ano,
                ld.Mes,
                gd.Nome AS Grupo,
                SUM(ld.Valor) AS Total
            FROM LancamentoDespesa ld
            INNER JOIN CategoriaDespesa cd ON cd.Id = ld.CategoriaDespesaId
            INNER JOIN GrupoDespesa gd     ON gd.Id = cd.GrupoDespesaId
            INNER JOIN AnoFiscal af        ON af.Id = ld.AnoFiscalId
                AND af.Ano = @Ano AND af.UsuarioId = @UsuarioId
            WHERE (@Mes IS NULL OR ld.Mes = @Mes)
            GROUP BY ld.Mes, gd.Nome
            ORDER BY Total DESC";

        return await db.QueryAsync<DespesaPorGrupo>(sql, new { Ano = ano, UsuarioId = usuarioId, Mes = mes });
    }
}
