using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Domain.Enums;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/passageiro")]
[Authorize(Roles = "Passageiro")]
public class PassageiroController : ControllerBase
{
    private readonly AppDbContext _db;
    public PassageiroController(AppDbContext db) => _db = db;

    // POST /api/passageiro/avaliacoes
    // Regra: avaliação só é permitida se a viagem estiver Concluida.
    [HttpPost("avaliacoes")]
    public async Task<IActionResult> Avaliar([FromBody] AvaliacaoDto dto)
    {
        var viagem = await _db.Viagens.FindAsync(dto.ViagemId);
        if (viagem is null)
            return NotFound(new { erro = "Viagem não encontrada." });
        if (viagem.StatusViagem != StatusViagem.Concluida)
            return BadRequest(new { erro = "Só é possível avaliar viagens concluídas." });
        if (!await _db.Passageiros.AnyAsync(p => p.Id == dto.PassageiroId))
            return NotFound(new { erro = "Passageiro não encontrado." });
        if (await _db.AvaliacoesViagem.AnyAsync(a => a.ViagemId == dto.ViagemId && a.PassageiroId == dto.PassageiroId))
            return Conflict(new { erro = "Você já avaliou esta viagem." });

        var avaliacao = new AvaliacaoViagem
        {
            ViagemId = dto.ViagemId,
            PassageiroId = dto.PassageiroId,
            Nota = dto.Nota,
            Comentario = dto.Comentario,
            AvaliadoEm = DateTime.UtcNow
        };
        _db.AvaliacoesViagem.Add(avaliacao);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { avaliacao.Id, avaliacao.Nota });
    }

    // GET /api/passageiro/notificacoes/{passageiroId}
    [HttpGet("notificacoes/{passageiroId:int}")]
    public async Task<IActionResult> GetNotificacoes(int passageiroId)
    {
        var notis = await _db.Notificacoes.Where(n => n.PassageiroId == passageiroId)
            .OrderByDescending(n => n.EnviadaEm)
            .Select(n => new { n.Id, n.Mensagem, n.Lida, n.EnviadaEm })
            .ToListAsync();
        return Ok(notis);
    }
}
