using CityMove.Domain.Entities;
using CityMove.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CityMove.Infrastructure.Data;

public static class DbSeeder
{
    public static readonly string[] Roles = { "Admin", "Motorista", "Fiscal", "Passageiro" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // ---- Roles ----
        foreach (var role in Roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // ---- Usuários base ----
        await EnsureUserAsync(userManager, "admin@citymove.com", "Admin@123", "Administrador CityMove", "Admin");
        var fiscalUser = await EnsureUserAsync(userManager, "fiscal@citymove.com", "Fiscal@123", "Fiscal CityMove", "Fiscal");
        var passUser = await EnsureUserAsync(userManager, "passageiro@citymove.com", "Passageiro@123", "Passageiro Teste", "Passageiro");

        // ---- Configurações do sistema ----
        if (!await db.ConfiguracoesSistema.AnyAsync())
        {
            db.ConfiguracoesSistema.AddRange(
                new ConfiguracaoSistema { Chave = "CidadePadrao", Valor = "Palmas", Descricao = "Cidade padrão para consultas de clima" },
                new ConfiguracaoSistema { Chave = "NomeSistema", Valor = "CityMove", Descricao = "Nome exibido do sistema" }
            );
            await db.SaveChangesAsync();
        }

        // ============================================================
        // OBS.: Linhas, rotas, paradas, veículos, horários, atribuições
        // e viagens NÃO são semeados — devem ser criados manualmente pelo
        // Admin no painel. Aqui só ficam prontos os usuários/perfis.
        // ============================================================
        var agora = DateTime.UtcNow;

        // ---- Fiscal (registro de domínio vinculado ao usuário) ----
        if (!await db.Fiscais.AnyAsync())
            db.Fiscais.Add(new Fiscal { UserId = fiscalUser.Id, Matricula = "FISC-001", Setor = "Centro" });

        // ---- Passageiro (registro de domínio vinculado ao usuário) ----
        var passageiro = await db.Passageiros.FirstOrDefaultAsync();
        if (passageiro is null)
        {
            passageiro = new Passageiro { UserId = passUser.Id, DataNascimento = new DateTime(1995, 5, 20), Telefone = "(63) 99999-0000" };
            db.Passageiros.Add(passageiro);
        }
        await db.SaveChangesAsync();

        // ---- Motoristas prontos para serem escalados (sem atribuição) ----
        if (!await db.Motoristas.AnyAsync())
        {
            for (int i = 1; i <= 3; i++)
            {
                var mUser = await EnsureUserAsync(userManager, $"motorista{i}@citymove.com", "Motorista@123", $"Motorista {i}", "Motorista");
                db.Motoristas.Add(new Motorista
                {
                    UserId = mUser.Id,
                    CNH = $"{10000000000L + i}",
                    CategoriaCNH = "D",
                    ValidadeCNH = agora.AddYears(3),
                    Disponivel = true
                });
            }
            await db.SaveChangesAsync();
        }

        // ---- Notificação de boas-vindas ao passageiro ----
        if (!await db.Notificacoes.AnyAsync())
        {
            db.Notificacoes.Add(new Notificacao
            {
                PassageiroId = passageiro.Id,
                Mensagem = "Bem-vindo ao CityMove! Não esqueça de avaliar suas viagens.",
                EnviadaEm = agora.AddHours(-1)
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string senha, string nome, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nome = nome,
                Role = role,
                Ativo = true
            };
            await userManager.CreateAsync(user, senha);
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}
