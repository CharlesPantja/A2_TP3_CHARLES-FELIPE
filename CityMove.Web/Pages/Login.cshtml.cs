using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApiClient _api;
    public LoginModel(ApiClient api) => _api = api;

    [BindProperty]
    public string Email { get; set; } = "";
    [BindProperty]
    public string Senha { get; set; } = "";
    public string? Erro { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var resultado = await _api.LoginAsync(Email, Senha);
        if (!resultado.Sucesso)
        {
            Erro = resultado.Erro ?? "Não foi possível entrar. Verifique suas credenciais.";
            return Page();
        }

        var roles = (resultado.Roles ?? Enumerable.Empty<string>()).ToList();
        HttpContext.Session.SetString("token", resultado.Token ?? "");
        HttpContext.Session.SetString("nome", resultado.Nome ?? "");
        HttpContext.Session.SetString("roles", string.Join(",", roles));

        // Cada papel vai para o seu próprio painel
        if (roles.Contains("Admin")) return RedirectToPage("/Painel");
        if (roles.Contains("Motorista")) return RedirectToPage("/Motorista");
        if (roles.Contains("Fiscal")) return RedirectToPage("/Fiscal");
        if (roles.Contains("Passageiro")) return RedirectToPage("/Passageiro");
        return RedirectToPage("/Index");
    }
}
