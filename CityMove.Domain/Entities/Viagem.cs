using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Viagem
{
    public int Id { get; set; }

    public int AtribuicaoId { get; set; }
    public int RotaId { get; set; }

    public DateTime HorarioPartida { get; set; }
    public DateTime? HorarioChegada { get; set; }

    public StatusViagem StatusViagem { get; set; } = StatusViagem.Planejada;

    public virtual AtribuicaoMotorista? Atribuicao { get; set; }
    public virtual Rota? Rota { get; set; }
    public virtual ICollection<RegistroGPS> RegistrosGPS { get; set; } = new List<RegistroGPS>();
    public virtual ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    public virtual ICollection<AvaliacaoViagem> Avaliacoes { get; set; } = new List<AvaliacaoViagem>();
}
