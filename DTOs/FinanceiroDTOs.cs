// ============================================================
// DTOs/FinanceiroDTOs.cs
// Objetos de transferência para requests e responses da API
// ============================================================

namespace FinanceiroAPI.DTOs;

// ---------- RECEITA ----------

public record LancamentoReceitaRequest(
    int AnoFiscalId,
    int CategoriaReceitaId,
    int? ClienteId,
    int Mes,
    decimal Valor,
    string? Observacao
);

public record LancamentoReceitaResponse(
    int Id,
    int Ano,
    string Grupo,
    string Categoria,
    string? Cliente,
    int Mes,
    string NomeMes,
    decimal Valor,
    string? Observacao,
    DateTime LancadoEm
);

public record ClienteRequest(
    string Nome,
    decimal? ValorMensal
);

public record ClienteResponse(
    int Id,
    string Nome,
    decimal? ValorMensal,
    bool Ativo
);

// ---------- DESPESA ----------

public record LancamentoDespesaRequest(
    int AnoFiscalId,
    int CategoriaDespesaId,
    int Mes,
    decimal Valor,
    string? Observacao
);

public record LancamentoDespesaResponse(
    int Id,
    int Ano,
    string Grupo,
    string Categoria,
    int Mes,
    string NomeMes,
    decimal Valor,
    string? Observacao,
    DateTime LancadoEm
);

// ---------- RESUMO ----------

public record ResumoMensalResponse(
    int Ano,
    int Mes,
    string NomeMes,
    decimal TotalReceita,
    decimal TotalDespesa,
    decimal SaldoMensal,
    decimal SaldoAcumulado
);

public record ResumoAnualResponse(
    int Ano,
    decimal TotalReceita,
    decimal TotalDespesa,
    decimal SaldoAnual,
    IEnumerable<ResumoMensalResponse> Meses,
    IEnumerable<DespesaGrupoResponse> DespesasPorGrupo
);

public record DespesaGrupoResponse(
    string Grupo,
    decimal Total,
    decimal PercentualSobreTotal
);

// ---------- AUTH ----------

public record LoginRequest(string Email, string Senha);
public record LoginResponse(string Token, string Perfil, DateTime Expira);

// ---------- USUARIOS (gerenciado pelo contador) ----------

public record UsuarioCreateRequest(string Email, string Senha);
public record UsuarioResponse(int Id, string Email, string Perfil, bool Ativo, DateTime CriadoEm);

// ---------- ANO FISCAL ----------

public record AnoFiscalRequest(int Ano, string? Descricao, decimal SaldoInicial = 0);

public record AnoFiscalResponse(int Id, int Ano, string? Descricao, decimal SaldoInicial, DateTime CriadoEm);
