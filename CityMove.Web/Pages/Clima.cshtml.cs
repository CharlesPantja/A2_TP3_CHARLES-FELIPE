using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class ClimaModel : PageModel
{
    private readonly ApiClient _api;
    public ClimaModel(ApiClient api) => _api = api;

    public string Cidade { get; set; } = "Palmas";
    public ClimaVm? Clima { get; set; }

    public async Task OnGetAsync(string? cidade)
    {
        if (!string.IsNullOrWhiteSpace(cidade))
            Cidade = cidade.Trim();

        Clima = await _api.GetClimaAsync(Cidade);
    }
}
