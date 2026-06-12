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
    public async Task<IActionResult> GetAll()
    {
        // Regra: motorista com CNH vencida fica automaticamente indisponível.
        var hoje = DateTime.Today;
        var vencidos = await _db.Motoristas.Where(m => m.Disponivel && m.ValidadeCNH < hoje).ToListAsync();
        if (vencidos.Count > 0)
        {
            foreach (var m in vencidos) m.Disponivel = false;
            await _db.SaveChangesAsync();
        }

        return Ok(await _db.Motoristas.Include(m => m.User)
            .Select(m => new { m.Id, m.CNH, m.CategoriaCNH, m.ValidadeCNH, m.Disponivel, Nome = m.User!.Nome, m.User.Email })
            .ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var m = await _db.Motoristas.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        return m is null ? NotFound() : Ok(new { m.Id, m.CNH, m.CategoriaCNH, m.ValidadeCNH, m.Disponivel, Nome = m.User!.Nome, m.User.Email });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MotoristaCreateDto dto)
    {
        if (dto.ValidadeCNH.Date < DateTime.Today)
            return BadRequest(new { erro = "A CNH está vencida. Não é possível cadastrar um motorista com a carteira vencida." });

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
    public async Task<IActionResult> Update(int id, [FromBody] MotoristaUpdateDto dto)
    {
        var motorista = await _db.Motoristas.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
        if (motorista is null) return NotFound();

        motorista.CNH = dto.CNH;
        motorista.CategoriaCNH = dto.CategoriaCNH;
        motorista.ValidadeCNH = dto.ValidadeCNH;
        motorista.Disponivel = dto.Disponivel;
        if (motorista.User is not null && !string.IsNullOrWhiteSpace(dto.Nome))
            motorista.User.Nome = dto.Nome;
        await _db.SaveChangesAsync();
        return Ok(new { motorista.Id, motorista.CNH, motorista.Disponivel });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var motorista = await _db.Motoristas.FindAsync(id);
        if (motorista is null) return NotFound();

        var temVinculos = await _db.AtribuicoesMotorista.AnyAsync(a => a.MotoristaId == id)
            || await _db.Ocorrencias.AnyAsync(o => o.MotoristaId == id)
            || await _db.Infracoes.AnyAsync(inf => inf.MotoristaId == id);
        if (temVinculos)
            return Conflict(new { erro = "Não é possível excluir: este motorista tem atribuições, ocorrências ou infrações vinculadas." });

        _db.Motoristas.Remove(motorista);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
