using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class AcompanharModel : PageModel
{
    private readonly ApiClient _api;
    public AcompanharModel(ApiClient api) => _api = api;

    public void OnGet() { }

    public async Task<IActionResult> OnGetPosicoesAsync()
    {
        var posicoes = await _api.GetPosicoesAsync();
        return new JsonResult(posicoes);
    }
}
