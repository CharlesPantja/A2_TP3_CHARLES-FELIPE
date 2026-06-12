using System.Security.Claims;
using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Domain.Enums;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/fiscal")]
[Authorize(Roles = "Fiscal")]
public class FiscalController : ControllerBase
{
    private readonly AppDbContext _db;
    public FiscalController(AppDbContext db) => _db = db;

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // GET /api/fiscal/contexto -> id do fiscal logado + listas para o formulário de infração
    [HttpGet("contexto")]
    public async Task<IActionResult> Contexto()
    {
        var fiscal = await _db.Fiscais.FirstOrDefaultAsync(f => f.UserId == UserId);
        var motoristas = await _db.Motoristas.Include(m => m.User)
            .Select(m => new { m.Id, Nome = m.User!.Nome }).ToListAsync();
        // Apenas veículos ATIVOS podem receber infração
        var veiculos = await _db.Veiculos.Where(v => v.StatusVeiculo == StatusVeiculo.Ativo)
            .Select(v => new { v.Id, v.Placa }).ToListAsync();
        return Ok(new { fiscalId = fiscal?.Id ?? 0, motoristas, veiculos });
    }

    // GET /api/fiscal/frota  -> situação atual da frota com última posição
    [HttpGet("frota")]
    public async Task<IActionResult> GetFrota()
    {
        var frota = await _db.Veiculos.Include(v => v.Linha)
            .Select(v => new
            {
                v.Id,
                v.Placa,
                v.Modelo,
                Status = v.StatusVeiculo.ToString(),
                Linha = v.Linha!.Nome,
                Motorista = _db.AtribuicoesMotorista.Where(a => a.VeiculoId == v.Id)
                    .OrderByDescending(a => a.DataHoraInicio)
                    .Select(a => a.Motorista!.User!.Nome).FirstOrDefault(),
                TemOcorrencia = _db.Ocorrencias.Any(o => o.Viagem!.Atribuicao!.VeiculoId == v.Id),
                UltimaPosicao = _db.RegistrosGPS.Where(g => g.VeiculoId == v.Id)
                    .OrderByDescending(g => g.CaptadoEm)
                    .Select(g => new { g.Latitude, g.Longitude, g.Velocidade, g.CaptadoEm })
                    .FirstOrDefault()
            })
            .ToListAsync();
        return Ok(frota);
    }

    // POST /api/fiscal/infracoes
    [HttpPost("infracoes")]
    public async Task<IActionResult> RegistrarInfracao([FromBody] InfracaoDto dto)
    {
        if (!await _db.Fiscais.AnyAsync(f => f.Id == dto.FiscalId))
            return NotFound(new { erro = "Fiscal não encontrado." });
        if (!await _db.Motoristas.AnyAsync(m => m.Id == dto.MotoristaId))
            return NotFound(new { erro = "Motorista não encontrado." });
        var veiculo = await _db.Veiculos.FindAsync(dto.VeiculoId);
        if (veiculo is null)
            return NotFound(new { erro = "Veículo não encontrado." });
        if (veiculo.StatusVeiculo != StatusVeiculo.Ativo)
            return BadRequest(new { erro = "Só é possível registrar infração de veículos com status Ativo." });

        var infracao = new Infracao
        {
            FiscalId = dto.FiscalId,
            MotoristaId = dto.MotoristaId,
            VeiculoId = dto.VeiculoId,
            TipoInfracao = dto.TipoInfracao,
            Descricao = dto.Descricao,
            StatusInfracao = StatusInfracao.Registrada,
            RegistradaEm = DateTime.UtcNow
        };
        _db.Infracoes.Add(infracao);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { infracao.Id, Status = infracao.StatusInfracao.ToString() });
    }
}
