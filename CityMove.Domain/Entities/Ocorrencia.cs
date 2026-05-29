using System.ComponentModel.DataAnnotations;
using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Ocorrencia
{
    public int Id { get; set; }

    public int ViagemId { get; set; }
    public int MotoristaId { get; set; }

    public TipoOcorrencia TipoOcorrencia { get; set; }

    [Required, StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    public StatusOcorrencia StatusOcorrencia { get; set; } = StatusOcorrencia.Aberta;

    public DateTime OcorridoEm { get; set; } = DateTime.UtcNow;

    public virtual Viagem? Viagem { get; set; }
    public virtual Motorista? Motorista { get; set; }
}
