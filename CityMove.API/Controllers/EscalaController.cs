using CityMove.API.Dtos;
using CityMove.Domain.Entities;
using CityMove.Domain.Enums;
using CityMove.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityMove.API.Controllers;

[ApiController]
[Route("api/escala")]
[Authorize(Roles = "Admin")]
public class EscalaController : ControllerBase
{
    private readonly AppDbContext _db;
    public EscalaController(AppDbContext db) => _db = db;

    // GET /api/escala -> lista de atribuições com status da viagem atual
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _db.AtribuicoesMotorista
            .OrderByDescending(a => a.DataHoraInicio)
            .Select(a => new
            {
                a.Id,
                motorista = a.Motorista!.User!.Nome,
                veiculo = a.Veiculo!.Placa,
                linha = a.Linha!.Nome,
                inicio = a.DataHoraInicio,
                viagemAtivaId = _db.Viagens
                    .Where(v => v.AtribuicaoId == a.Id && v.StatusViagem == StatusViagem.EmAndamento)
                    .Select(v => (int?)v.Id).FirstOrDefault()
            })
            .ToListAsync();
        return Ok(lista);
    }

    // GET /api/escala/opcoes -> listas para os formulários
    [HttpGet("opcoes")]
    public async Task<IActionResult> GetOpcoes()
    {
        var hoje = DateTime.Today;
        // Apenas motoristas APTOS (disponíveis e com CNH válida)
        var motoristas = await _db.Motoristas.Include(m => m.User)
            .Where(m => m.Disponivel && m.ValidadeCNH >= hoje)
            .Select(m => new { m.Id, Nome = m.User!.Nome }).ToListAsync();
        // Apenas veículos ATIVOS
        var veiculos = await _db.Veiculos.Where(v => v.StatusVeiculo == StatusVeiculo.Ativo)
            .Select(v => new { v.Id, v.Placa }).ToListAsync();
        // Apenas linhas e rotas ATIVAS
        var linhas = await _db.Linhas.Where(l => l.Ativa)
            .Select(l => new { l.Id, Nome = l.Codigo + " - " + l.Nome }).ToListAsync();
        var rotas = await _db.Rotas.Where(r => r.Ativa)
            .Select(r => new { r.Id, r.LinhaId, r.Descricao }).ToListAsync();
        return Ok(new { motoristas, veiculos, linhas, rotas });
    }

    // POST /api/escala/atribuir -> vincula motorista a veículo e linha
    [HttpPost("atribuir")]
    public async Task<IActionResult> Atribuir([FromBody] AtribuirDto dto)
    {
        if (!await _db.Motoristas.AnyAsync(m => m.Id == dto.MotoristaId))
            return NotFound(new { erro = "Motorista não encontrado." });
        if (!await _db.Veiculos.AnyAsync(v => v.Id == dto.VeiculoId))
            return NotFound(new { erro = "Veículo não encontrado." });
        if (!await _db.Linhas.AnyAsync(l => l.Id == dto.LinhaId))
            return NotFound(new { erro = "Linha não encontrada." });

        var atrib = new AtribuicaoMotorista
        {
            MotoristaId = dto.MotoristaId,
            VeiculoId = dto.VeiculoId,
            LinhaId = dto.LinhaId,
            DataHoraInicio = DateTime.UtcNow
        };
        _db.AtribuicoesMotorista.Add(atrib);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { atrib.Id });
    }

    // POST /api/escala/iniciar-viagem -> cria uma viagem EmAndamento para a atribuição
    [HttpPost("iniciar-viagem")]
    public async Task<IActionResult> IniciarViagem([FromBody] IniciarViagemDto dto)
    {
        var atrib = await _db.AtribuicoesMotorista.FindAsync(dto.AtribuicaoId);
        if (atrib is null) return NotFound(new { erro = "Atribuição não encontrada." });
        if (!await _db.Rotas.AnyAsync(r => r.Id == dto.RotaId))
            return NotFound(new { erro = "Rota não encontrada." });

        var jaTem = await _db.Viagens.AnyAsync(v => v.AtribuicaoId == dto.AtribuicaoId && v.StatusViagem == StatusViagem.EmAndamento);
        if (jaTem) return Conflict(new { erro = "Esta atribuição já tem uma viagem em andamento." });

        var viagem = new Viagem
        {
            AtribuicaoId = dto.AtribuicaoId,
            RotaId = dto.RotaId,
            HorarioPartida = DateTime.UtcNow,
            StatusViagem = StatusViagem.EmAndamento
        };
        _db.Viagens.Add(viagem);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { viagem.Id });
    }

    // POST /api/escala/concluir-viagem -> encerra a viagem
    [HttpPost("concluir-viagem")]
    public async Task<IActionResult> ConcluirViagem([FromBody] ConcluirViagemDto dto)
    {
        var viagem = await _db.Viagens.FindAsync(dto.ViagemId);
        if (viagem is null) return NotFound(new { erro = "Viagem não encontrada." });

        viagem.StatusViagem = StatusViagem.Concluida;
        viagem.HorarioChegada = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { viagem.Id, Status = viagem.StatusViagem.ToString() });
    }

    // DELETE /api/escala/{id} -> exclui a escala (atribuição) e todo o histórico de viagens dela
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var atrib = await _db.AtribuicoesMotorista.FindAsync(id);
        if (atrib is null) return NotFound(new { erro = "Escala não encontrada." });

        var viagemIds = await _db.Viagens.Where(v => v.AtribuicaoId == id).Select(v => v.Id).ToListAsync();
        if (viagemIds.Count > 0)
        {
            _db.RegistrosGPS.RemoveRange(_db.RegistrosGPS.Where(g => viagemIds.Contains(g.ViagemId)));
            _db.Ocorrencias.RemoveRange(_db.Ocorrencias.Where(o => viagemIds.Contains(o.ViagemId)));
            _db.AvaliacoesViagem.RemoveRange(_db.AvaliacoesViagem.Where(a => viagemIds.Contains(a.ViagemId)));
            _db.Viagens.RemoveRange(_db.Viagens.Where(v => v.AtribuicaoId == id));
        }

        _db.AtribuicoesMotorista.Remove(atrib);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
