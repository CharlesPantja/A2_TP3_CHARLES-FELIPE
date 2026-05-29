using System.ComponentModel.DataAnnotations;

namespace CityMove.Domain.Entities;

public class ConfiguracaoSistema
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Chave { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Valor { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descricao { get; set; }

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
