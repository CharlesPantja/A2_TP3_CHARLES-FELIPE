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
[Route("api/passageiro")]
[Authorize(Roles = "Passageiro")]
public class PassageiroController : ControllerBase
{
    private readonly AppDbContext _db;
    public PassageiroController(AppDbContext db) => _db = db;

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // GET /api/passageiro/contexto -> id do passageiro logado, viagens avaliáveis e notificações
    [HttpGet("contexto")]
    public async Task<IActionResult> Contexto()
    {
        var pas = await _db.Passageiros.FirstOrDefaultAsync(p => p.UserId == UserId);
        if (pas is null)
            return Ok(new { passageiroId = 0, notificacoes = Array.Empty<object>() });

        var notificacoes = await _db.Notificacoes.Where(n => n.PassageiroId == pas.Id)
            .OrderByDescending(n => n.EnviadaEm)
            .Select(n => new { n.Mensagem, n.EnviadaEm, n.Lida })
            .ToListAsync();

        return Ok(new { passageiroId = pas.Id, notificacoes });
    }

    // POST /api/passageiro/avaliacoes -> avaliação livre (linha + placa informadas, sem vínculo com viagem)
    [HttpPost("avaliacoes")]
    public async Task<IActionResult> Avaliar([FromBody] AvaliacaoDto dto)
    {
        if (!await _db.Passageiros.AnyAsync(p => p.Id == dto.PassageiroId))
            return NotFound(new { erro = "Passageiro não encontrado." });

        var avaliacao = new AvaliacaoViagem
        {
            ViagemId = null,
            PassageiroId = dto.PassageiroId,
            Linha = dto.Linha,
            Placa = dto.Placa,
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
