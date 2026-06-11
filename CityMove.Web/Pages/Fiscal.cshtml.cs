using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class FiscalModel : PageModel
{
    private readonly ApiClient _api;
    public FiscalModel(ApiClient api) => _api = api;

    public FiscalContextoVm? Ctx { get; set; }
    public List<FrotaItemVm> Frota { get; set; } = new();
    public static readonly string[] Tipos = { "DesvioPadrao", "AtrasoRecorrente", "VelocidadeExcessiva", "Outro" };

    [BindProperty] public int FiscalId { get; set; }
    [BindProperty] public int MotoristaId { get; set; }
    [BindProperty] public int VeiculoId { get; set; }
    [BindProperty] public string TipoInfracao { get; set; } = "DesvioPadrao";
    [BindProperty] public string Descricao { get; set; } = "";

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");
    private bool IsFiscal => (HttpContext.Session.GetString("roles") ?? "").Contains("Fiscal");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        if (!IsFiscal) return RedirectToPage("/Index");
        Ctx = await _api.GetFiscalContextoAsync(Token);
        Frota = await _api.GetFrotaAsync(Token);
        return Page();
    }

    public async Task<IActionResult> OnPostInfracaoAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { fiscalId = FiscalId, motoristaId = MotoristaId, veiculoId = VeiculoId, tipoInfracao = TipoInfracao, descricao = Descricao };
        var res = await _api.RegistrarInfracaoAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Infração registrada.";
        else Erro = res.Erro;
        return RedirectToPage();
    }
}
