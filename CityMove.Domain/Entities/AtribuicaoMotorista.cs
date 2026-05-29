namespace CityMove.Domain.Entities;

public class AtribuicaoMotorista
{
    public int Id { get; set; }

    public int MotoristaId { get; set; }
    public int VeiculoId { get; set; }
    public int LinhaId { get; set; }

    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }

    public virtual Motorista? Motorista { get; set; }
    public virtual Veiculo? Veiculo { get; set; }
    public virtual Linha? Linha { get; set; }
    public virtual ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
}
