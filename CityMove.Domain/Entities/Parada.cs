using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityMove.Domain.Entities;

public class Parada
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Endereco { get; set; } = string.Empty;

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    public bool Ativa { get; set; } = true;

    public virtual ICollection<RotaParada> RotaParadas { get; set; } = new List<RotaParada>();
}
