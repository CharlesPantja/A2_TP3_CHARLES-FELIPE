using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class Notificacao
{
    public int Id { get; set; }

    public int PassageiroId { get; set; }
    public int? LinhaId { get; set; }

    [Required, StringLength(500)]
    public string Mensagem { get; set; } = string.Empty;

    public bool Lida { get; set; } = false;

    public DateTime EnviadaEm { get; set; } = DateTime.UtcNow;

    public virtual Passageiro? Passageiro { get; set; }
    public virtual Linha? Linha { get; set; }
}
