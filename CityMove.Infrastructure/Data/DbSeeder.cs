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

        // ---- Usuário Admin ----
        var admin = await EnsureUserAsync(userManager, "admin@citymove.com", "Admin@123", "Administrador CityMove", "Admin");

        // ---- Configurações do sistema ----
        if (!await db.ConfiguracoesSistema.AnyAsync())
        {
            db.ConfiguracoesSistema.AddRange(
                new ConfiguracaoSistema { Chave = "CidadePadrao", Valor = "Palmas", Descricao = "Cidade padrão para consultas de clima" },
                new ConfiguracaoSistema { Chave = "NomeSistema", Valor = "CityMove", Descricao = "Nome exibido do sistema" }
            );
            await db.SaveChangesAsync();
        }

        // ---- Dados operacionais de exemplo ----
        if (!await db.Linhas.AnyAsync())
        {
            var linha = new Linha
            {
                Codigo = "L001",
                Nome = "Centro - Plano Diretor Sul",
                TipoLinha = TipoLinha.Urbana,
                Tarifa = 4.50m,
                Ativa = true
            };
            db.Linhas.Add(linha);
            await db.SaveChangesAsync();

            var rota = new Rota { LinhaId = linha.Id, Descricao = "Centro até Plano Sul", Sentido = "Ida", Ativa = true };
            db.Rotas.Add(rota);

            var p1 = new Parada { Nome = "Terminal Central", Endereco = "Av. JK, Centro - Palmas/TO", Latitude = -10.184m, Longitude = -48.333m };
            var p2 = new Parada { Nome = "Praça dos Girassóis", Endereco = "Praça dos Girassóis - Palmas/TO", Latitude = -10.187m, Longitude = -48.333m };
            db.Paradas.AddRange(p1, p2);
            await db.SaveChangesAsync();

            db.RotaParadas.AddRange(
                new RotaParada { RotaId = rota.Id, ParadaId = p1.Id, Ordem = 1, TempoEstimado = 0 },
                new RotaParada { RotaId = rota.Id, ParadaId = p2.Id, Ordem = 2, TempoEstimado = 8 }
            );

            db.Veiculos.Add(new Veiculo
            {
                LinhaId = linha.Id,
                Placa = "ABC1D23",
                Modelo = "Torino",
                Marca = "Marcopolo",
                Capacidade = 42,
                StatusVeiculo = StatusVeiculo.Ativo
            });

            db.Horarios.AddRange(
                new Horario { LinhaId = linha.Id, HoraPartida = new TimeOnly(6, 0), DiaSemana = DiaSemana.Segunda, Ativo = true },
                new Horario { LinhaId = linha.Id, HoraPartida = new TimeOnly(7, 30), DiaSemana = DiaSemana.Segunda, Ativo = true }
            );

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
