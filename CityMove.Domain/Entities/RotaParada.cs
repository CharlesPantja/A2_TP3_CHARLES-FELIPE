namespace CityMove.Domain.Entities;

public class RotaParada
{
    public int Id { get; set; }

    public int RotaId { get; set; }
    public int ParadaId { get; set; }

    /// <summary>Ordem da parada dentro da rota.</summary>
    public int Ordem { get; set; }

    /// <summary>Tempo estimado em minutos até esta parada.</summary>
    public int TempoEstimado { get; set; }

    public virtual Rota? Rota { get; set; }
    public virtual Parada? Parada { get; set; }
}
