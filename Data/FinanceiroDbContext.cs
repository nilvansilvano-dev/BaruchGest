using Microsoft.EntityFrameworkCore;
using FinanceiroAPI.Models;

namespace FinanceiroAPI.Data;

public class FinanceiroDbContext : DbContext
{
    public FinanceiroDbContext(DbContextOptions<FinanceiroDbContext> options) : base(options) { }

    public DbSet<AnoFiscal> AnosFiscais => Set<AnoFiscal>();
    public DbSet<GrupoReceita> GruposReceita => Set<GrupoReceita>();
    public DbSet<CategoriaReceita> CategoriasReceita => Set<CategoriaReceita>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<LancamentoReceita> LancamentosReceita => Set<LancamentoReceita>();
    public DbSet<GrupoDespesa> GruposDespesa => Set<GrupoDespesa>();
    public DbSet<CategoriaDespesa> CategoriasDespesa => Set<CategoriaDespesa>();
    public DbSet<LancamentoDespesa> LancamentosDespesa => Set<LancamentoDespesa>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<AnoFiscal>(e =>
        {
            e.ToTable("AnoFiscal");
            e.HasKey(x => x.Id);
            e.Property(x => x.Ano).IsRequired();
            e.Property(x => x.SaldoInicial).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            e.Property(x => x.CriadoEm).HasDefaultValueSql("GETDATE()");
        });

        model.Entity<GrupoReceita>(e =>
        {
            e.ToTable("GrupoReceita");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Ativo).HasDefaultValue(true);
        });

        model.Entity<CategoriaReceita>(e =>
        {
            e.ToTable("CategoriaReceita");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Ativo).HasDefaultValue(true);
            e.HasOne(x => x.Grupo)
             .WithMany()
             .HasForeignKey(x => x.GrupoReceitaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.ValorMensal).HasColumnType("decimal(18,2)");
            e.Property(x => x.Ativo).HasDefaultValue(true);
            e.Property(x => x.CriadoEm).HasDefaultValueSql("GETDATE()");
        });

        model.Entity<LancamentoReceita>(e =>
        {
            e.ToTable("LancamentoReceita");
            e.HasKey(x => x.Id);
            e.Property(x => x.Valor).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.Observacao).HasMaxLength(500);
            e.Property(x => x.LancadoEm).HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.AnoFiscal)
             .WithMany()
             .HasForeignKey(x => x.AnoFiscalId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Categoria)
             .WithMany()
             .HasForeignKey(x => x.CategoriaReceitaId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Cliente)
             .WithMany()
             .HasForeignKey(x => x.ClienteId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        model.Entity<GrupoDespesa>(e =>
        {
            e.ToTable("GrupoDespesa");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Ativo).HasDefaultValue(true);
        });

        model.Entity<CategoriaDespesa>(e =>
        {
            e.ToTable("CategoriaDespesa");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Ativo).HasDefaultValue(true);
            e.HasOne(x => x.Grupo)
             .WithMany()
             .HasForeignKey(x => x.GrupoDespesaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<LancamentoDespesa>(e =>
        {
            e.ToTable("LancamentoDespesa");
            e.HasKey(x => x.Id);
            e.Property(x => x.Valor).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.Observacao).HasMaxLength(500);
            e.Property(x => x.LancadoEm).HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.AnoFiscal)
             .WithMany()
             .HasForeignKey(x => x.AnoFiscalId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Categoria)
             .WithMany()
             .HasForeignKey(x => x.CategoriaDespesaId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
