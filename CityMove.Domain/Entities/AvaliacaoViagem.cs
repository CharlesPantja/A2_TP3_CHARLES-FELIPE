using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class AvaliacaoViagem
{
    public int Id { get; set; }

    public int ViagemId { get; set; }
    public int PassageiroId { get; set; }

    [Range(1, 5)]
    public int Nota { get; set; }

    [StringLength(500)]
    public string? Comentario { get; set; }

    public DateTime AvaliadoEm { get; set; } = DateTime.UtcNow;

    public virtual Viagem? Viagem { get; set; }
    public virtual Passageiro? Passageiro { get; set; }
}
