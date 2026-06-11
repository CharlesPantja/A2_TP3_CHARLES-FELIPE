using System.Text.Json;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class PainelModel : PageModel
{
    private readonly ApiClient _api;
    public PainelModel(ApiClient api) => _api = api;

    public string Nome { get; set; } = "";
    public JsonElement? Relatorio { get; set; }
    public string? Erro { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        Nome = HttpContext.Session.GetString("nome") ?? "";
        Relatorio = await _api.GetRelatoriosAsync(token);
        if (Relatorio is null)
            Erro = "Não foi possível carregar os relatórios. Verifique se você tem perfil Admin e se a API está em execução.";

        return Page();
    }

    public static string Num(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) ? v.ToString() : "-";
}
