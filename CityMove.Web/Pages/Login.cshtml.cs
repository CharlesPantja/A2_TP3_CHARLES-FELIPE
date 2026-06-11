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

        HttpContext.Session.SetString("token", resultado.Token ?? "");
        HttpContext.Session.SetString("nome", resultado.Nome ?? "");
        HttpContext.Session.SetString("roles", string.Join(",", resultado.Roles ?? Enumerable.Empty<string>()));
        return RedirectToPage("/Painel");
    }
}
