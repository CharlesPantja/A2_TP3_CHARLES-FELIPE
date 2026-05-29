using System.ComponentModel.DataAnnotations.Schema;

namespace CityMove.Domain.Entities;

public class RegistroGPS
{
    public int Id { get; set; }

    public int VeiculoId { get; set; }
    public int ViagemId { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    /// <summary>Velocidade em km/h.</summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal Velocidade { get; set; }

    public DateTime CaptadoEm { get; set; } = DateTime.UtcNow;

    public virtual Veiculo? Veiculo { get; set; }
    public virtual Viagem? Viagem { get; set; }
}
