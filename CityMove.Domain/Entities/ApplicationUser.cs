using Microsoft.AspNetCore.Identity;

namespace CityMove.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>Papel principal do usuário: Admin, Motorista, Fiscal ou Passageiro.</summary>
    public string Role { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public virtual Motorista? Motorista { get; set; }
    public virtual Passageiro? Passageiro { get; set; }
    public virtual Fiscal? Fiscal { get; set; }
}
