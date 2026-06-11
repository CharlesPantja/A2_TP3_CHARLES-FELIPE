using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<LinhaVm> Linhas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Linhas = await _api.GetLinhasAsync();
    }
}
