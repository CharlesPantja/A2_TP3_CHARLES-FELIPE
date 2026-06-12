using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class RegistrarModel : PageModel
{
    private readonly ApiClient _api;
    public RegistrarModel(ApiClient api) => _api = api;

    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Senha { get; set; } = "";

    public string? Erro { get; set; }
    public string? Sucesso { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var res = await _api.RegistrarPassageiroAsync(Nome, Email, Senha);
        if (res.Sucesso)
        {
            Sucesso = "Cadastro realizado com sucesso! Agora é só entrar.";
            Nome = Email = Senha = "";
            return Page();
        }
        Erro = res.Erro;
        return Page();
    }
}
