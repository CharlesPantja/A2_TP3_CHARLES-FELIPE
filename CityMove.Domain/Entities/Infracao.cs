using System.ComponentModel.DataAnnotations;
using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Infracao
{
    public int Id { get; set; }

    public int FiscalId { get; set; }
    public int MotoristaId { get; set; }
    public int VeiculoId { get; set; }

    public TipoInfracao TipoInfracao { get; set; }

    [Required, StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    public StatusInfracao StatusInfracao { get; set; } = StatusInfracao.Registrada;

    public DateTime RegistradaEm { get; set; } = DateTime.UtcNow;

    public virtual Fiscal? Fiscal { get; set; }
    public virtual Motorista? Motorista { get; set; }
    public virtual Veiculo? Veiculo { get; set; }
}
