using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class AvaliacaoViagem
{
    public int Id { get; set; }

    /// <summary>Opcional: a avaliação pode ser livre, sem vínculo com uma viagem registrada.</summary>
    public int? ViagemId { get; set; }

    public int PassageiroId { get; set; }

    /// <summary>Linha informada pelo passageiro (texto livre).</summary>
    [StringLength(120)]
    public string? Linha { get; set; }

    /// <summary>Placa do veículo informada pelo passageiro (texto livre).</summary>
    [StringLength(10)]
    public string? Placa { get; set; }

    [Range(1, 5)]
    public int Nota { get; set; }

    [StringLength(500)]
    public string? Comentario { get; set; }

    public DateTime AvaliadoEm { get; set; } = DateTime.UtcNow;

    public virtual Viagem? Viagem { get; set; }
    public virtual Passageiro? Passageiro { get; set; }
}
