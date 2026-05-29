using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class Rota
{
    public int Id { get; set; }

    public int LinhaId { get; set; }

    [Required, StringLength(160)]
    public string Descricao { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Sentido { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public virtual Linha? Linha { get; set; }
    public virtual ICollection<RotaParada> RotaParadas { get; set; } = new List<RotaParada>();
    public virtual ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
}
