using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Linhas;

public class DetailsModel : PageModel
{
    private readonly ApiClient _api;
    public DetailsModel(ApiClient api) => _api = api;

    public int LinhaId { get; set; }
    public List<HorarioVm> Horarios { get; set; } = new();
    public List<ParadaVm> Paradas { get; set; } = new();

    public async Task OnGetAsync(int id)
    {
        LinhaId = id;
        Horarios = await _api.GetHorariosAsync(id);
        Paradas = await _api.GetParadasAsync(id);
    }
}
