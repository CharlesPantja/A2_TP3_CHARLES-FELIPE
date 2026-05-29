using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MotoristasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MotoristasController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Motoristas.Include(m => m.User)
            .Select(m => new { m.Id, m.CNH, m.CategoriaCNH, m.ValidadeCNH, m.Disponivel, Nome = m.User!.Nome, m.User.Email })
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var m = await _db.Motoristas.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        return m is null ? NotFound() : Ok(new { m.Id, m.CNH, m.CategoriaCNH, m.ValidadeCNH, m.Disponivel, Nome = m.User!.Nome, m.User.Email });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MotoristaCreateDto dto)
    {
        if (await _db.Motoristas.AnyAsync(m => m.CNH == dto.CNH))
            return Conflict(new { erro = "Já existe um motorista com esta CNH." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            Nome = dto.Nome,
            Role = "Motorista",
            Ativo = true
        };
        var result = await _userManager.CreateAsync(user, dto.Senha);
        if (!result.Succeeded)
            return BadRequest(new { erros = result.Errors.Select(e => e.Description) });
        await _userManager.AddToRoleAsync(user, "Motorista");

        var motorista = new Motorista
        {
            UserId = user.Id,
            CNH = dto.CNH,
            CategoriaCNH = dto.CategoriaCNH,
            ValidadeCNH = dto.ValidadeCNH,
            Disponivel = dto.Disponivel
        };
        _db.Motoristas.Add(motorista);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = motorista.Id }, new { motorista.Id, motorista.CNH });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MotoristaCreateDto dto)
    {
        var motorista = await _db.Motoristas.FindAsync(id);
        if (motorista is null) return NotFound();

        motorista.CNH = dto.CNH;
        motorista.CategoriaCNH = dto.CategoriaCNH;
        motorista.ValidadeCNH = dto.ValidadeCNH;
        motorista.Disponivel = dto.Disponivel;
        await _db.SaveChangesAsync();
        return Ok(new { motorista.Id, motorista.CNH, motorista.Disponivel });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var motorista = await _db.Motoristas.FindAsync(id);
        if (motorista is null) return NotFound();
        _db.Motoristas.Remove(motorista);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
