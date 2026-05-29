using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Domain.Enums;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/motorista")]
[Authorize(Roles = "Motorista")]
public class MotoristaController : ControllerBase
{
    private readonly AppDbContext _db;
    public MotoristaController(AppDbContext db) => _db = db;

    // POST /api/motorista/gps
    // Regra: só registra GPS se a viagem estiver EmAndamento.
    [HttpPost("gps")]
    public async Task<IActionResult> RegistrarGps([FromBody] GpsDto dto)
    {
        var viagem = await _db.Viagens.FindAsync(dto.ViagemId);
        if (viagem is null)
            return NotFound(new { erro = "Viagem não encontrada." });
        if (viagem.StatusViagem != StatusViagem.EmAndamento)
            return BadRequest(new { erro = "GPS só pode ser registrado em viagens com status EmAndamento." });

        var registro = new RegistroGPS
        {
            VeiculoId = dto.VeiculoId,
            ViagemId = dto.ViagemId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Velocidade = dto.Velocidade,
            CaptadoEm = DateTime.UtcNow
        };
        _db.RegistrosGPS.Add(registro);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { registro.Id, registro.CaptadoEm });
    }

    // POST /api/motorista/ocorrencias
    [HttpPost("ocorrencias")]
    public async Task<IActionResult> RegistrarOcorrencia([FromBody] OcorrenciaDto dto)
    {
        if (!await _db.Viagens.AnyAsync(v => v.Id == dto.ViagemId))
            return NotFound(new { erro = "Viagem não encontrada." });
        if (!await _db.Motoristas.AnyAsync(m => m.Id == dto.MotoristaId))
            return NotFound(new { erro = "Motorista não encontrado." });

        var ocorrencia = new Ocorrencia
        {
            ViagemId = dto.ViagemId,
            MotoristaId = dto.MotoristaId,
            TipoOcorrencia = dto.TipoOcorrencia,
            Descricao = dto.Descricao,
            StatusOcorrencia = StatusOcorrencia.Aberta,
            OcorridoEm = DateTime.UtcNow
        };
        _db.Ocorrencias.Add(ocorrencia);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { ocorrencia.Id, Status = ocorrencia.StatusOcorrencia.ToString() });
    }
}
