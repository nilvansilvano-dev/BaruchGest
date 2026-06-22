using FinanceiroAPI.DTOs;
using FinanceiroAPI.Repositories;

namespace FinanceiroAPI.Services;

public class ResumoService
{
    private static readonly string[] Meses =
    {
        "", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    private readonly IFinanceiroRepository _repo;

    public ResumoService(IFinanceiroRepository repo) => _repo = repo;

    public async Task<ResumoAnualResponse> GetResumoAnualAsync(int ano, int usuarioId)
    {
        var anoFiscal = (await _repo.GetAnosFiscaisAsync(usuarioId)).FirstOrDefault(a => a.Ano == ano);
        decimal saldoInicial = anoFiscal?.SaldoInicial ?? 0m;

        var meses  = (await _repo.GetResumoMensalAsync(ano, usuarioId)).ToList();
        var grupos = (await _repo.GetDespesaPorGrupoAsync(ano, usuarioId)).ToList();

        decimal saldoAcumulado = saldoInicial;
        var mesesResponse = meses.Select(m =>
        {
            saldoAcumulado += m.SaldoMensal;
            return new ResumoMensalResponse(
                m.Ano, m.Mes, Meses[m.Mes],
                m.TotalReceita, m.TotalDespesa,
                m.SaldoMensal, saldoAcumulado);
        }).ToList();

        decimal totalDespesa = grupos.Sum(g => g.Total);
        var gruposResponse = grupos
            .GroupBy(g => g.Grupo)
            .Select(g => new DespesaGrupoResponse(
                g.Key,
                g.Sum(x => x.Total),
                totalDespesa > 0 ? Math.Round(g.Sum(x => x.Total) / totalDespesa * 100, 1) : 0
            ))
            .OrderByDescending(g => g.Total)
            .ToList();

        return new ResumoAnualResponse(
            ano,
            mesesResponse.Sum(m => m.TotalReceita),
            mesesResponse.Sum(m => m.TotalDespesa),
            mesesResponse.Sum(m => m.SaldoMensal),
            mesesResponse,
            gruposResponse
        );
    }

    public async Task<object?> GetResumoMesAsync(int ano, int mes, int usuarioId)
    {
        var anoFiscal = (await _repo.GetAnosFiscaisAsync(usuarioId)).FirstOrDefault(a => a.Ano == ano);
        decimal saldoInicial = anoFiscal?.SaldoInicial ?? 0m;

        var todosMeses = (await _repo.GetResumoMensalAsync(ano, usuarioId)).ToList();
        var grupos     = (await _repo.GetDespesaPorGrupoAsync(ano, usuarioId, mes)).ToList();

        var mesData = todosMeses.FirstOrDefault(m => m.Mes == mes);
        if (mesData is null) return null;

        decimal saldoAcumulado = saldoInicial +
            todosMeses.Where(m => m.Mes <= mes).Sum(m => m.SaldoMensal);

        decimal totalDespesa = grupos.Sum(g => g.Total);

        return new
        {
            Ano          = ano,
            Mes          = mes,
            NomeMes      = Meses[mes],
            TotalReceita = mesData.TotalReceita,
            TotalDespesa = mesData.TotalDespesa,
            SaldoMensal  = mesData.SaldoMensal,
            SaldoAcumulado = saldoAcumulado,
            DespesasPorGrupo = grupos.Select(g => new DespesaGrupoResponse(
                g.Grupo, g.Total,
                totalDespesa > 0 ? Math.Round(g.Total / totalDespesa * 100, 1) : 0
            ))
        };
    }
}
