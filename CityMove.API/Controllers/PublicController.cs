using CityMove.API.Services;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly OpenWeatherService _weather;
    private readonly NominatimService _nominatim;

    public PublicController(AppDbContext db, OpenWeatherService weather, NominatimService nominatim)
    {
        _db = db;
        _weather = weather;
        _nominatim = nominatim;
    }

    // GET /api/public/linhas
    [HttpGet("linhas")]
    public async Task<IActionResult> GetLinhas()
    {
        var linhas = await _db.Linhas.Where(l => l.Ativa)
            .Select(l => new { l.Id, l.Codigo, l.Nome, TipoLinha = l.TipoLinha.ToString(), l.Tarifa })
            .ToListAsync();
        return Ok(linhas);
    }

    // GET /api/public/linhas/{id}/horarios
    [HttpGet("linhas/{id:int}/horarios")]
    public async Task<IActionResult> GetHorarios(int id)
    {
        if (!await _db.Linhas.AnyAsync(l => l.Id == id))
            return NotFound(new { erro = "Linha não encontrada." });

        var horarios = await _db.Horarios.Where(h => h.LinhaId == id && h.Ativo)
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraPartida)
            .Select(h => new { h.Id, h.HoraPartida, DiaSemana = h.DiaSemana.ToString() })
            .ToListAsync();
        return Ok(horarios);
    }

    // GET /api/public/linhas/{id}/paradas
    [HttpGet("linhas/{id:int}/paradas")]
    public async Task<IActionResult> GetParadas(int id)
    {
        if (!await _db.Linhas.AnyAsync(l => l.Id == id))
            return NotFound(new { erro = "Linha não encontrada." });

        var paradas = await _db.RotaParadas
            .Where(rp => rp.Rota!.LinhaId == id)
            .OrderBy(rp => rp.Ordem)
            .Select(rp => new
            {
                rp.Ordem,
                rp.TempoEstimado,
                Parada = new { rp.Parada!.Id, rp.Parada.Nome, rp.Parada.Endereco, rp.Parada.Latitude, rp.Parada.Longitude }
            })
            .ToListAsync();
        return Ok(paradas);
    }

    // GET /api/public/veiculos/{id}/posicao
    [HttpGet("veiculos/{id:int}/posicao")]
    public async Task<IActionResult> GetPosicao(int id)
    {
        var ultimo = await _db.RegistrosGPS.Where(g => g.VeiculoId == id)
            .OrderByDescending(g => g.CaptadoEm)
            .Select(g => new { g.VeiculoId, g.ViagemId, g.Latitude, g.Longitude, g.Velocidade, g.CaptadoEm })
            .FirstOrDefaultAsync();

        if (ultimo is null)
            return NotFound(new { erro = "Nenhuma posição registrada para este veículo." });
        return Ok(ultimo);
    }

    // GET /api/public/veiculos/posicoes  (última posição de cada veículo ativo - alimenta o mapa)
    [HttpGet("veiculos/posicoes")]
    public async Task<IActionResult> GetPosicoes()
    {
        // Pega o registro mais recente de cada veículo (subconsulta correlacionada - traduz bem no EF Core).
        var posicoes = await _db.RegistrosGPS
            .Where(g => !_db.RegistrosGPS.Any(g2 => g2.VeiculoId == g.VeiculoId && g2.CaptadoEm > g.CaptadoEm))
            .Select(g => new
            {
                g.VeiculoId,
                Placa = g.Veiculo!.Placa,
                LinhaCodigo = g.Veiculo.Linha!.Codigo,
                LinhaNome = g.Veiculo.Linha.Nome,
                g.Latitude,
                g.Longitude,
                g.Velocidade,
                g.CaptadoEm
            })
            .ToListAsync();

        return Ok(posicoes);
    }

    // GET /api/public/geocodificar?endereco=...  (consome OpenStreetMap/Nominatim - API de terceiros)
    [HttpGet("geocodificar")]
    public async Task<IActionResult> Geocodificar([FromQuery] string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            return BadRequest(new { erro = "Informe um endereço." });

        var resultado = await _nominatim.GeocodificarAsync(endereco);
        return Ok(resultado);
    }

    // GET /api/public/clima/{cidade}  (consome OpenWeatherMap - API de terceiros)
    [HttpGet("clima/{cidade}")]
    public async Task<IActionResult> GetClima(string cidade)
    {
        var resultado = await _weather.ObterClimaAsync(cidade);
        return Ok(resultado);
    }
}
