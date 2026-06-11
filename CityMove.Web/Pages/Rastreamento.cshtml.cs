using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class RastreamentoModel : PageModel
{
    private readonly ApiClient _api;
    public RastreamentoModel(ApiClient api) => _api = api;

    public void OnGet() { }

    // Chamado via JavaScript (mesma origem) para evitar CORS: /Rastreamento?handler=Posicoes
    public async Task<IActionResult> OnGetPosicoesAsync()
    {
        var posicoes = await _api.GetPosicoesAsync();
        return new JsonResult(posicoes);
    }
}
