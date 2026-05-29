using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class Fiscal
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Matricula { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Setor { get; set; }

    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<Infracao> Infracoes { get; set; } = new List<Infracao>();
}
