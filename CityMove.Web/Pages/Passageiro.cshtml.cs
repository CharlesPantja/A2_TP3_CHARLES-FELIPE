using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class PassageiroModel : PageModel
{
    private readonly ApiClient _api;
    public PassageiroModel(ApiClient api) => _api = api;

    public PassageiroContextoVm? Ctx { get; set; }
    public string Nome => HttpContext.Session.GetString("nome") ?? "Passageiro";

    [BindProperty] public int PassageiroId { get; set; }
    [BindProperty] public string? Linha { get; set; }
    [BindProperty] public string? Placa { get; set; }
    [BindProperty] public int Nota { get; set; } = 5;
    [BindProperty] public string? Comentario { get; set; }

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");
    private bool IsPassageiro => (HttpContext.Session.GetString("roles") ?? "").Contains("Passageiro");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        if (!IsPassageiro) return RedirectToPage("/Index");
        Ctx = await _api.GetPassageiroContextoAsync(Token);
        return Page();
    }

    public async Task<IActionResult> OnPostAvaliarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { passageiroId = PassageiroId, linha = Linha, placa = Placa, nota = Nota, comentario = Comentario };
        var res = await _api.AvaliarViagemAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Obrigado pela avaliação!";
        else Erro = res.Erro;
        return RedirectToPage();
    }
}
