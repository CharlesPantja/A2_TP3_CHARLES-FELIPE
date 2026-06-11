using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Admin;

public class LinhasModel : PageModel
{
    private readonly ApiClient _api;
    public LinhasModel(ApiClient api) => _api = api;

    public List<LinhaAdminVm> Linhas { get; set; } = new();
    public bool Editando => Id is not null && Id != 0;

    [BindProperty] public int? Id { get; set; }
    [BindProperty] public string Codigo { get; set; } = "";
    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public string TipoLinha { get; set; } = "Urbana";
    [BindProperty] public decimal Tarifa { get; set; }
    [BindProperty] public bool Ativa { get; set; } = true;

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    public static readonly string[] Tipos = { "Urbana", "Intermunicipal", "Escolar", "Especial" };

    private string? Token => HttpContext.Session.GetString("token");

    public async Task<IActionResult> OnGetAsync(int? editId)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        Linhas = await _api.GetLinhasAdminAsync(Token);

        if (editId is not null)
        {
            var l = Linhas.FirstOrDefault(x => x.Id == editId);
            if (l is not null)
            {
                Id = l.Id; Codigo = l.Codigo; Nome = l.Nome;
                TipoLinha = l.TipoLinha; Tarifa = l.Tarifa; Ativa = l.Ativa;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var dto = new { codigo = Codigo, nome = Nome, tipoLinha = TipoLinha, tarifa = Tarifa, ativa = Ativa };
        var res = await _api.SalvarLinhaAsync(Token, Id, dto);

        if (res.Sucesso) Mensagem = Editando ? "Linha atualizada com sucesso." : "Linha cadastrada com sucesso.";
        else Erro = res.Erro;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var res = await _api.ExcluirLinhaAsync(Token, id);
        if (res.Sucesso) Mensagem = "Linha removida.";
        else Erro = res.Erro;

        return RedirectToPage();
    }
}
