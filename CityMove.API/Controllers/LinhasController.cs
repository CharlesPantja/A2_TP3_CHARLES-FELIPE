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
public class LinhasController : ControllerBase
{
    private readonly AppDbContext _db;
    public LinhasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Linhas.ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var linha = await _db.Linhas.FindAsync(id);
        return linha is null ? NotFound() : Ok(linha);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LinhaCreateDto dto)
    {
        if (await _db.Linhas.AnyAsync(l => l.Codigo == dto.Codigo))
            return Conflict(new { erro = "Já existe uma linha com este código." });

        var linha = new Linha
        {
            Codigo = dto.Codigo,
            Nome = dto.Nome,
            TipoLinha = dto.TipoLinha,
            Tarifa = dto.Tarifa,
            Ativa = dto.Ativa
        };
        _db.Linhas.Add(linha);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = linha.Id }, linha);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LinhaCreateDto dto)
    {
        var linha = await _db.Linhas.FindAsync(id);
        if (linha is null) return NotFound();

        linha.Codigo = dto.Codigo;
        linha.Nome = dto.Nome;
        linha.TipoLinha = dto.TipoLinha;
        linha.Tarifa = dto.Tarifa;
        linha.Ativa = dto.Ativa;
        await _db.SaveChangesAsync();
        return Ok(linha);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var linha = await _db.Linhas.FindAsync(id);
        if (linha is null) return NotFound();
        _db.Linhas.Remove(linha);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
