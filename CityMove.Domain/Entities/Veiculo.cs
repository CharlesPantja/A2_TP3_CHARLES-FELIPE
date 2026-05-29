using System.ComponentModel.DataAnnotations;
using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Veiculo
{
    public int Id { get; set; }

    public int LinhaId { get; set; }

    [Required, StringLength(8)]
    public string Placa { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    public int Capacidade { get; set; }

    public StatusVeiculo StatusVeiculo { get; set; } = StatusVeiculo.Ativo;

    public virtual Linha? Linha { get; set; }
    public virtual ICollection<AtribuicaoMotorista> Atribuicoes { get; set; } = new List<AtribuicaoMotorista>();
    public virtual ICollection<RegistroGPS> RegistrosGPS { get; set; } = new List<RegistroGPS>();
    public virtual ICollection<Infracao> Infracoes { get; set; } = new List<Infracao>();
}
