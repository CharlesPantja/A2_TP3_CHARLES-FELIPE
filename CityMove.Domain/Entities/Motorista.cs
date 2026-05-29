using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class Motorista
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(11)]
    public string CNH { get; set; } = string.Empty;

    [Required, StringLength(5)]
    public string CategoriaCNH { get; set; } = string.Empty;

    public DateTime ValidadeCNH { get; set; }

    public bool Disponivel { get; set; } = true;

    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<AtribuicaoMotorista> Atribuicoes { get; set; } = new List<AtribuicaoMotorista>();
    public virtual ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    public virtual ICollection<Infracao> Infracoes { get; set; } = new List<Infracao>();
}
