// ============================================================
// Models/Entities.cs
// Entidades do domínio — espelham as tabelas do SQL Server
// ============================================================

namespace FinanceiroAPI.Models;

public class AnoFiscal
{
    public int Id { get; set; }
    public int Ano { get; set; }
    public string? Descricao { get; set; }
    public decimal SaldoInicial { get; set; }
    public DateTime CriadoEm { get; set; }
}

// ---------- RECEITA ----------

public class GrupoReceita
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public class CategoriaReceita
{
    public int Id { get; set; }
    public int GrupoReceitaId { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public GrupoReceita? Grupo { get; set; }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal? ValorMensal { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}

public class LancamentoReceita
{
    public int Id { get; set; }
    public int AnoFiscalId { get; set; }
    public int CategoriaReceitaId { get; set; }
    public int? ClienteId { get; set; }
    public int Mes { get; set; }
    public decimal Valor { get; set; }
    public string? Observacao { get; set; }
    public DateTime LancadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    // Navegação (retorno enriquecido)
    public CategoriaReceita? Categoria { get; set; }
    public Cliente? Cliente { get; set; }
    public AnoFiscal? AnoFiscal { get; set; }
}

// ---------- DESPESA ----------

public class GrupoDespesa
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public class CategoriaDespesa
{
    public int Id { get; set; }
    public int GrupoDespesaId { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public GrupoDespesa? Grupo { get; set; }
}

public class LancamentoDespesa
{
    public int Id { get; set; }
    public int AnoFiscalId { get; set; }
    public int CategoriaDespesaId { get; set; }
    public int Mes { get; set; }
    public decimal Valor { get; set; }
    public string? Observacao { get; set; }
    public DateTime LancadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    // Navegação
    public CategoriaDespesa? Categoria { get; set; }
    public AnoFiscal? AnoFiscal { get; set; }
}

// ---------- AUTH ----------

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Perfil { get; set; } = "usuario"; // "usuario" | "contador"
    public DateTime CriadoEm { get; set; }
}

// ---------- RESUMO (view) ----------

public class ResumoMensal
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal TotalReceita { get; set; }
    public decimal TotalDespesa { get; set; }
    public decimal SaldoMensal { get; set; }
}

public class DespesaPorGrupo
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Grupo { get; set; } = "";
    public decimal Total { get; set; }
}
