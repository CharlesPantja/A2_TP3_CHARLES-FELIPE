using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RotasController : ControllerBase
{
    private readonly AppDbContext _db;
    public RotasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Rotas
            .Select(r => new { r.Id, r.LinhaId, r.Descricao, r.Sentido, r.Ativa })
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var rota = await _db.Rotas.FindAsync(id);
        return rota is null ? NotFound() : Ok(rota);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RotaCreateDto dto)
    {
        if (!await _db.Linhas.AnyAsync(l => l.Id == dto.LinhaId))
            return BadRequest(new { erro = "Linha informada não existe." });

        var rota = new Rota
        {
            LinhaId = dto.LinhaId,
            Descricao = dto.Descricao,
            Sentido = dto.Sentido,
            Ativa = dto.Ativa
        };
        _db.Rotas.Add(rota);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = rota.Id }, rota);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RotaCreateDto dto)
    {
        var rota = await _db.Rotas.FindAsync(id);
        if (rota is null) return NotFound();

        if (!await _db.Linhas.AnyAsync(l => l.Id == dto.LinhaId))
            return BadRequest(new { erro = "Linha informada não existe." });

        rota.LinhaId = dto.LinhaId;
        rota.Descricao = dto.Descricao;
        rota.Sentido = dto.Sentido;
        rota.Ativa = dto.Ativa;
        await _db.SaveChangesAsync();
        return Ok(rota);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rota = await _db.Rotas.FindAsync(id);
        if (rota is null) return NotFound();
        _db.Rotas.Remove(rota);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
