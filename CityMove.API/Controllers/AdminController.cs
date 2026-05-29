using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // GET /api/admin/relatorios -> visão geral consolidada do sistema
    [HttpGet("relatorios")]
    public async Task<IActionResult> GetRelatorios()
    {
        var relatorio = new
        {
            totalLinhas = await _db.Linhas.CountAsync(),
            linhasAtivas = await _db.Linhas.CountAsync(l => l.Ativa),
            totalVeiculos = await _db.Veiculos.CountAsync(),
            veiculosPorStatus = await _db.Veiculos
                .GroupBy(v => v.StatusVeiculo)
                .Select(g => new { Status = g.Key.ToString(), Total = g.Count() })
                .ToListAsync(),
            totalMotoristas = await _db.Motoristas.CountAsync(),
            motoristasDisponiveis = await _db.Motoristas.CountAsync(m => m.Disponivel),
            totalViagens = await _db.Viagens.CountAsync(),
            viagensPorStatus = await _db.Viagens
                .GroupBy(v => v.StatusViagem)
                .Select(g => new { Status = g.Key.ToString(), Total = g.Count() })
                .ToListAsync(),
            ocorrenciasAbertas = await _db.Ocorrencias.CountAsync(o => o.StatusOcorrencia == CityMove.Domain.Enums.StatusOcorrencia.Aberta),
            totalInfracoes = await _db.Infracoes.CountAsync(),
            notaMediaAvaliacoes = await _db.AvaliacoesViagem.AnyAsync()
                ? await _db.AvaliacoesViagem.AverageAsync(a => (double)a.Nota)
                : 0
        };
        return Ok(relatorio);
    }
}
