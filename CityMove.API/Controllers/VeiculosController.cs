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
public class VeiculosController : ControllerBase
{
    private readonly AppDbContext _db;
    public VeiculosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Veiculos.ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var v = await _db.Veiculos.FindAsync(id);
        return v is null ? NotFound() : Ok(v);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VeiculoCreateDto dto)
    {
        if (!await _db.Linhas.AnyAsync(l => l.Id == dto.LinhaId))
            return BadRequest(new { erro = "Linha informada não existe." });
        if (await _db.Veiculos.AnyAsync(v => v.Placa == dto.Placa))
            return Conflict(new { erro = "Já existe um veículo com esta placa." });

        var veiculo = new Veiculo
        {
            LinhaId = dto.LinhaId,
            Placa = dto.Placa,
            Modelo = dto.Modelo,
            Marca = dto.Marca,
            Capacidade = dto.Capacidade,
            StatusVeiculo = dto.StatusVeiculo
        };
        _db.Veiculos.Add(veiculo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = veiculo.Id }, veiculo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VeiculoCreateDto dto)
    {
        var veiculo = await _db.Veiculos.FindAsync(id);
        if (veiculo is null) return NotFound();

        veiculo.LinhaId = dto.LinhaId;
        veiculo.Placa = dto.Placa;
        veiculo.Modelo = dto.Modelo;
        veiculo.Marca = dto.Marca;
        veiculo.Capacidade = dto.Capacidade;
        veiculo.StatusVeiculo = dto.StatusVeiculo;
        await _db.SaveChangesAsync();
        return Ok(veiculo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var veiculo = await _db.Veiculos.FindAsync(id);
        if (veiculo is null) return NotFound();
        _db.Veiculos.Remove(veiculo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
