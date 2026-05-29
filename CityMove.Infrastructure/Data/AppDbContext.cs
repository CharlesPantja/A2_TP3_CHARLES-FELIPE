using CityMove.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CityMove.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Motorista> Motoristas => Set<Motorista>();
    public DbSet<Passageiro> Passageiros => Set<Passageiro>();
    public DbSet<Fiscal> Fiscais => Set<Fiscal>();
    public DbSet<Linha> Linhas => Set<Linha>();
    public DbSet<Rota> Rotas => Set<Rota>();
    public DbSet<Parada> Paradas => Set<Parada>();
    public DbSet<RotaParada> RotaParadas => Set<RotaParada>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<AtribuicaoMotorista> AtribuicoesMotorista => Set<AtribuicaoMotorista>();
    public DbSet<Viagem> Viagens => Set<Viagem>();
    public DbSet<RegistroGPS> RegistrosGPS => Set<RegistroGPS>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Infracao> Infracoes => Set<Infracao>();
    public DbSet<AvaliacaoViagem> AvaliacoesViagem => Set<AvaliacaoViagem>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema => Set<ConfiguracaoSistema>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- Relações 1:1 entre ApplicationUser e perfis ----
        builder.Entity<Motorista>()
            .HasOne(m => m.User).WithOne(u => u.Motorista)
            .HasForeignKey<Motorista>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Passageiro>()
            .HasOne(p => p.User).WithOne(u => u.Passageiro)
            .HasForeignKey<Passageiro>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Fiscal>()
            .HasOne(f => f.User).WithOne(u => u.Fiscal)
            .HasForeignKey<Fiscal>(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Índices únicos ----
        builder.Entity<Motorista>().HasIndex(m => m.CNH).IsUnique();
        builder.Entity<Fiscal>().HasIndex(f => f.Matricula).IsUnique();
        builder.Entity<Linha>().HasIndex(l => l.Codigo).IsUnique();
        builder.Entity<Veiculo>().HasIndex(v => v.Placa).IsUnique();
        builder.Entity<ConfiguracaoSistema>().HasIndex(c => c.Chave).IsUnique();
        builder.Entity<RotaParada>().HasIndex(rp => new { rp.RotaId, rp.Ordem }).IsUnique();

        // ---- Evitar múltiplos caminhos de cascade (SQL Server) ----
        builder.Entity<Veiculo>()
            .HasOne(v => v.Linha).WithMany(l => l.Veiculos)
            .HasForeignKey(v => v.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Rota>()
            .HasOne(r => r.Linha).WithMany(l => l.Rotas)
            .HasForeignKey(r => r.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Horario>()
            .HasOne(h => h.Linha).WithMany(l => l.Horarios)
            .HasForeignKey(h => h.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RotaParada>()
            .HasOne(rp => rp.Rota).WithMany(r => r.RotaParadas)
            .HasForeignKey(rp => rp.RotaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RotaParada>()
            .HasOne(rp => rp.Parada).WithMany(p => p.RotaParadas)
            .HasForeignKey(rp => rp.ParadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AtribuicaoMotorista>()
            .HasOne(a => a.Motorista).WithMany(m => m.Atribuicoes)
            .HasForeignKey(a => a.MotoristaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AtribuicaoMotorista>()
            .HasOne(a => a.Veiculo).WithMany(v => v.Atribuicoes)
            .HasForeignKey(a => a.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AtribuicaoMotorista>()
            .HasOne(a => a.Linha).WithMany()
            .HasForeignKey(a => a.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Viagem>()
            .HasOne(v => v.Atribuicao).WithMany(a => a.Viagens)
            .HasForeignKey(v => v.AtribuicaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Viagem>()
            .HasOne(v => v.Rota).WithMany(r => r.Viagens)
            .HasForeignKey(v => v.RotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RegistroGPS>()
            .HasOne(g => g.Veiculo).WithMany(v => v.RegistrosGPS)
            .HasForeignKey(g => g.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RegistroGPS>()
            .HasOne(g => g.Viagem).WithMany(v => v.RegistrosGPS)
            .HasForeignKey(g => g.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ocorrencia>()
            .HasOne(o => o.Viagem).WithMany(v => v.Ocorrencias)
            .HasForeignKey(o => o.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ocorrencia>()
            .HasOne(o => o.Motorista).WithMany(m => m.Ocorrencias)
            .HasForeignKey(o => o.MotoristaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Infracao>()
            .HasOne(i => i.Fiscal).WithMany(f => f.Infracoes)
            .HasForeignKey(i => i.FiscalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Infracao>()
            .HasOne(i => i.Motorista).WithMany(m => m.Infracoes)
            .HasForeignKey(i => i.MotoristaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Infracao>()
            .HasOne(i => i.Veiculo).WithMany(v => v.Infracoes)
            .HasForeignKey(i => i.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AvaliacaoViagem>()
            .HasOne(a => a.Viagem).WithMany(v => v.Avaliacoes)
            .HasForeignKey(a => a.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AvaliacaoViagem>()
            .HasOne(a => a.Passageiro).WithMany(p => p.Avaliacoes)
            .HasForeignKey(a => a.PassageiroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notificacao>()
            .HasOne(n => n.Passageiro).WithMany(p => p.Notificacoes)
            .HasForeignKey(n => n.PassageiroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notificacao>()
            .HasOne(n => n.Linha).WithMany(l => l.Notificacoes)
            .HasForeignKey(n => n.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
