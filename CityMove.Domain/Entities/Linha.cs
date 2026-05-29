using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CityMove.Domain.Enums;

namespace CityMove.Domain.Entities;

public class Linha
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    public TipoLinha TipoLinha { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Tarifa { get; set; }

    public bool Ativa { get; set; } = true;

    public virtual ICollection<Rota> Rotas { get; set; } = new List<Rota>();
    public virtual ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();
    public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
