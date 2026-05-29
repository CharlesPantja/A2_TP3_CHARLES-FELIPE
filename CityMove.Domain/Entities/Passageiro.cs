using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class Passageiro
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<AvaliacaoViagem> Avaliacoes { get; set; } = new List<AvaliacaoViagem>();
    public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
