using System.ComponentModel.DataAnnotations;
using CityMove.Domain.Enums;

namespace CityMove.API.Dtos;

// ---- Autenticação ----
public record LoginDto([Required] string Email, [Required] string Senha);
public record RegistroDto([Required] string Nome, [Required, EmailAddress] string Email, [Required] string Senha, [Required] string Role);
public record TokenResponseDto(string Token, DateTime ExpiraEm, string Nome, string Email, IEnumerable<string> Roles);

// ---- Linha ----
public record LinhaCreateDto(
    [Required, StringLength(20)] string Codigo,
    [Required, StringLength(120)] string Nome,
    TipoLinha TipoLinha,
    [Range(0, 1000)] decimal Tarifa,
    bool Ativa);

// ---- Veículo ----
public record VeiculoCreateDto(
    int LinhaId,
    [Required, StringLength(8)] string Placa,
    [Required] string Modelo,
    [Required] string Marca,
    [Range(1, 200)] int Capacidade,
    StatusVeiculo StatusVeiculo);

// ---- Motorista ----
public record MotoristaCreateDto(
    [Required] string Nome,
    [Required, EmailAddress] string Email,
    [Required] string Senha,
    [Required, StringLength(11)] string CNH,
    [Required] string CategoriaCNH,
    DateTime ValidadeCNH,
    bool Disponivel);

// ---- Execução (Motorista) ----
public record GpsDto(int VeiculoId, int ViagemId, decimal Latitude, decimal Longitude, decimal Velocidade);
public record OcorrenciaDto(int ViagemId, int MotoristaId, TipoOcorrencia TipoOcorrencia, [Required] string Descricao);

// ---- Fiscalização ----
public record InfracaoDto(int FiscalId, int MotoristaId, int VeiculoId, TipoInfracao TipoInfracao, [Required] string Descricao);

// ---- Passageiro ----
public record AvaliacaoDto(int ViagemId, int PassageiroId, [Range(1, 5)] int Nota, string? Comentario);
