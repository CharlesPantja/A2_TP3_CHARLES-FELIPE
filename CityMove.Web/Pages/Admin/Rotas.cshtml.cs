using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Admin;

public class RotasModel : PageModel
{
    private readonly ApiClient _api;
    public RotasModel(ApiClient api) => _api = api;

    public List<RotaAdminVm> Rotas { get; set; } = new();
    public List<LinhaAdminVm> Linhas { get; set; } = new();
    public bool Editando => Id is not null && Id != 0;

    public static readonly string[] Sentidos = { "Ida", "Volta", "Circular" };

    [BindProperty] public int? Id { get; set; }
    [BindProperty] public int LinhaId { get; set; }
    [BindProperty] public string Descricao { get; set; } = "";
    [BindProperty] public string Sentido { get; set; } = "Ida";
    [BindProperty] public bool Ativa { get; set; } = true;

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");

    public async Task<IActionResult> OnGetAsync(int? editId)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        Rotas = await _api.GetRotasAdminAsync(Token);
        Linhas = await _api.GetLinhasAdminAsync(Token);

        if (editId is not null)
        {
            var r = Rotas.FirstOrDefault(x => x.Id == editId);
            if (r is not null)
            {
                Id = r.Id; LinhaId = r.LinhaId; Descricao = r.Descricao;
                Sentido = r.Sentido; Ativa = r.Ativa;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var dto = new { linhaId = LinhaId, descricao = Descricao, sentido = Sentido, ativa = Ativa };
        var res = await _api.SalvarRotaAsync(Token, Id, dto);

        if (res.Sucesso) Mensagem = Editando ? "Rota atualizada." : "Rota cadastrada.";
        else Erro = res.Erro;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var res = await _api.ExcluirRotaAsync(Token, id);
        if (res.Sucesso) Mensagem = "Rota removida.";
        else Erro = res.Erro;

        return RedirectToPage();
    }
}
