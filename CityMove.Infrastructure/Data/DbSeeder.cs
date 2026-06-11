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

        // ---- Dados operacionais de exemplo ----
        if (await db.Linhas.AnyAsync()) return;

        // Definição declarativa das linhas (coordenadas reais aproximadas de Palmas/TO).
        var defs = new[]
        {
            new LinhaDef("L001", "Centro - Plano Diretor Sul", TipoLinha.Urbana, 4.50m, "ABC1D23", "Marcopolo", "Torino",
                new[]
                {
                    ("Terminal Central",     "Av. JK, Centro - Palmas/TO",          -10.1840m, -48.3336m),
                    ("Praça dos Girassóis",  "Praça dos Girassóis - Palmas/TO",     -10.1870m, -48.3330m),
                    ("Av. Teotônio Segurado","Av. Teotônio Segurado - Palmas/TO",   -10.2300m, -48.3290m),
                    ("Terminal Sul",         "Terminal Sul, Plano Diretor Sul",     -10.2700m, -48.3250m),
                }),
            new LinhaDef("L002", "Centro - Plano Diretor Norte", TipoLinha.Urbana, 4.50m, "DEF2G45", "Mercedes-Benz", "Caio Apache",
                new[]
                {
                    ("Terminal Central",     "Av. JK, Centro - Palmas/TO",          -10.1840m, -48.3336m),
                    ("Feira da 304 Norte",   "Quadra 304 Norte - Palmas/TO",        -10.1650m, -48.3360m),
                    ("UFT Norte",            "Universidade Federal - Palmas/TO",    -10.1720m, -48.3580m),
                    ("Terminal Norte",       "Terminal Norte, Plano Diretor Norte", -10.1500m, -48.3400m),
                }),
            new LinhaDef("L003", "Taquaralto - Centro", TipoLinha.Urbana, 5.00m, "GHI3J67", "Volkswagen", "Neobus",
                new[]
                {
                    ("Terminal Taquaralto",  "Av. Tocantins, Taquaralto - Palmas/TO", -10.3200m, -48.3100m),
                    ("Aureny III",           "Aureny III - Palmas/TO",                -10.3000m, -48.3200m),
                    ("Av. Teotônio Segurado","Av. Teotônio Segurado - Palmas/TO",     -10.2300m, -48.3290m),
                    ("Terminal Central",     "Av. JK, Centro - Palmas/TO",            -10.1840m, -48.3336m),
                }),
            new LinhaDef("L010", "Palmas - Porto Nacional", TipoLinha.Intermunicipal, 12.00m, "JKL4M89", "Scania", "Comil Campione",
                new[]
                {
                    ("Terminal Central",     "Av. JK, Centro - Palmas/TO",          -10.1840m, -48.3336m),
                    ("Saída Sul (TO-050)",   "Rodovia TO-050 - Palmas/TO",          -10.2900m, -48.3200m),
                    ("Luzimangues",          "Luzimangues - Porto Nacional/TO",     -10.2950m, -48.4100m),
                    ("Rodoviária Porto Nac.","Rodoviária - Porto Nacional/TO",      -10.7080m, -48.4170m),
                }),
        };

        var diasUteis = new[] { DiaSemana.Segunda, DiaSemana.Terca, DiaSemana.Quarta, DiaSemana.Quinta, DiaSemana.Sexta };
        var agora = DateTime.UtcNow;
        int motoristaIdx = 1;

        foreach (var d in defs)
        {
            var linha = new Linha
            {
                Codigo = d.Codigo, Nome = d.Nome, TipoLinha = d.Tipo, Tarifa = d.Tarifa, Ativa = true
            };
            db.Linhas.Add(linha);
            await db.SaveChangesAsync();

            var rota = new Rota { LinhaId = linha.Id, Descricao = $"{d.Paradas.First().Item1} → {d.Paradas.Last().Item1}", Sentido = "Ida", Ativa = true };
            db.Rotas.Add(rota);
            await db.SaveChangesAsync();

            // Paradas + RotaParadas
            int ordem = 1;
            foreach (var (nome, end, lat, lng) in d.Paradas)
            {
                var parada = new Parada { Nome = nome, Endereco = end, Latitude = lat, Longitude = lng, Ativa = true };
                db.Paradas.Add(parada);
                await db.SaveChangesAsync();
                db.RotaParadas.Add(new RotaParada { RotaId = rota.Id, ParadaId = parada.Id, Ordem = ordem, TempoEstimado = (ordem - 1) * 7 });
                ordem++;
            }

            // Veículo
            var veiculo = new Veiculo
            {
                LinhaId = linha.Id, Placa = d.Placa, Modelo = d.Modelo, Marca = d.Marca,
                Capacidade = 42, StatusVeiculo = StatusVeiculo.Ativo
            };
            db.Veiculos.Add(veiculo);

            // Horários
            db.Horarios.AddRange(diasUteis.SelectMany(dia => new[]
            {
                new Horario { LinhaId = linha.Id, HoraPartida = new TimeOnly(6, 0),  DiaSemana = dia, Ativo = true },
                new Horario { LinhaId = linha.Id, HoraPartida = new TimeOnly(12, 0), DiaSemana = dia, Ativo = true },
                new Horario { LinhaId = linha.Id, HoraPartida = new TimeOnly(18, 0), DiaSemana = dia, Ativo = true },
            }));
            await db.SaveChangesAsync();

            // Motorista (usuário Identity + registro Motorista)
            var email = $"motorista{motoristaIdx}@citymove.com";
            var mUser = await EnsureUserAsync(userManager, email, "Motorista@123", $"Motorista {motoristaIdx} - {d.Codigo}", "Motorista");
            var motorista = new Motorista
            {
                UserId = mUser.Id, CNH = $"{10000000000L + motoristaIdx}", CategoriaCNH = "D",
                ValidadeCNH = agora.AddYears(3), Disponivel = true
            };
            db.Motoristas.Add(motorista);
            await db.SaveChangesAsync();

            // Atribuição + Viagem em andamento
            var atrib = new AtribuicaoMotorista
            {
                MotoristaId = motorista.Id, VeiculoId = veiculo.Id, LinhaId = linha.Id,
                DataHoraInicio = agora.AddHours(-1)
            };
            db.AtribuicoesMotorista.Add(atrib);
            await db.SaveChangesAsync();

            var viagem = new Viagem
            {
                AtribuicaoId = atrib.Id, RotaId = rota.Id,
                HorarioPartida = agora.AddMinutes(-30), StatusViagem = StatusViagem.EmAndamento
            };
            db.Viagens.Add(viagem);
            await db.SaveChangesAsync();

            // Trilha de GPS: interpola entre a primeira e a última parada (10 pontos).
            var (latI, lngI) = (d.Paradas.First().Item3, d.Paradas.First().Item4);
            var (latF, lngF) = (d.Paradas.Last().Item3, d.Paradas.Last().Item4);
            const int pontos = 10;
            var rnd = new Random(linha.Id * 7 + 13);
            for (int i = 0; i < pontos; i++)
            {
                decimal t = i / (decimal)(pontos - 1);
                var lat = latI + (latF - latI) * t;
                var lng = lngI + (lngF - lngI) * t;
                db.RegistrosGPS.Add(new RegistroGPS
                {
                    VeiculoId = veiculo.Id,
                    ViagemId = viagem.Id,
                    Latitude = Math.Round(lat, 6),
                    Longitude = Math.Round(lng, 6),
                    Velocidade = 25 + rnd.Next(0, 20),
                    CaptadoEm = agora.AddMinutes(-(pontos - 1 - i) * 3) // o último ponto é o mais recente
                });
            }
            await db.SaveChangesAsync();

            motoristaIdx++;
        }

        // ---- Fiscal (registro de domínio vinculado ao usuário) ----
        var fiscal = new Fiscal { UserId = fiscalUser.Id, Matricula = "FISC-001", Setor = "Centro" };
        db.Fiscais.Add(fiscal);

        // ---- Passageiro (registro de domínio vinculado ao usuário) ----
        var passageiro = new Passageiro { UserId = passUser.Id, DataNascimento = new DateTime(1995, 5, 20), Telefone = "(63) 99999-0000" };
        db.Passageiros.Add(passageiro);
        await db.SaveChangesAsync();

        // ---- Uma viagem CONCLUÍDA para o passageiro poder avaliar ----
        var primeiraAtrib = await db.AtribuicoesMotorista.OrderBy(a => a.Id).FirstAsync();
        var primeiraRota = await db.Rotas.OrderBy(r => r.Id).FirstAsync();
        db.Viagens.Add(new Viagem
        {
            AtribuicaoId = primeiraAtrib.Id,
            RotaId = primeiraRota.Id,
            HorarioPartida = agora.AddHours(-3),
            HorarioChegada = agora.AddHours(-2),
            StatusViagem = StatusViagem.Concluida
        });

        // ---- Notificações de exemplo para o passageiro ----
        var primeiraLinha = await db.Linhas.OrderBy(l => l.Id).FirstAsync();
        db.Notificacoes.AddRange(
            new Notificacao { PassageiroId = passageiro.Id, LinhaId = primeiraLinha.Id, Mensagem = "Linha L001 terá horário extra hoje às 22h.", EnviadaEm = agora.AddHours(-5) },
            new Notificacao { PassageiroId = passageiro.Id, Mensagem = "Bem-vindo ao CityMove! Não esqueça de avaliar suas viagens.", EnviadaEm = agora.AddHours(-1) }
        );
        await db.SaveChangesAsync();
    }

    private record LinhaDef(
        string Codigo, string Nome, TipoLinha Tipo, decimal Tarifa,
        string Placa, string Marca, string Modelo,
        (string, string, decimal, decimal)[] Paradas);

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
