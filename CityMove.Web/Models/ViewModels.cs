namespace CityMove.Web.Models;

public record LinhaVm(int Id, string Codigo, string Nome, string TipoLinha, decimal Tarifa);

public record HorarioVm(int Id, string HoraPartida, string DiaSemana);

public record ParadaInfoVm(int Id, string Nome, string Endereco, decimal Latitude, decimal Longitude);

public record ParadaVm(int Ordem, int TempoEstimado, ParadaInfoVm Parada);

public record ClimaVm(string? Cidade, double? Temperatura, double? Sensacao, int? Umidade, string? Descricao, double? Vento, string? Erro);

public record LoginResultVm(bool Sucesso, string? Token, string? Nome, IEnumerable<string>? Roles, string? Erro);

// ---------- CRUD Admin ----------
public record LinhaAdminVm(int Id, string Codigo, string Nome, string TipoLinha, decimal Tarifa, bool Ativa);

public record VeiculoAdminVm(int Id, int LinhaId, string Placa, string Modelo, string Marca, int Capacidade, string StatusVeiculo);

public record RotaAdminVm(int Id, int LinhaId, string Descricao, string Sentido, bool Ativa);

public record MotoristaAdminVm(int Id, string CNH, string CategoriaCNH, DateTime ValidadeCNH, bool Disponivel, string? Nome, string? Email);

/// <summary>Resultado padrão de operações de escrita (POST/PUT/DELETE) na API.</summary>
public record ApiResult(bool Sucesso, string? Erro);

// ---------- Rastreamento (GPS em tempo real) ----------
public record PosicaoVm(
    int VeiculoId, string? Placa, string? LinhaCodigo, string? LinhaNome,
    decimal Latitude, decimal Longitude, decimal Velocidade, DateTime CaptadoEm);
