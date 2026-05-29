using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Horario
{
    public int Id { get; set; }

    public int LinhaId { get; set; }

    public TimeOnly HoraPartida { get; set; }

    public DiaSemana DiaSemana { get; set; }

    public bool Ativo { get; set; } = true;

    public virtual Linha? Linha { get; set; }
}
