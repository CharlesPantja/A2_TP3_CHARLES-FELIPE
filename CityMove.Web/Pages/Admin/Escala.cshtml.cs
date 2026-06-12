using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Admin;

public class EscalaModel : PageModel
{
    private readonly ApiClient _api;
    public EscalaModel(ApiClient api) => _api = api;

    public List<EscalaItemVm> Escala { get; set; } = new();
    public EscalaOpcoesVm? Opcoes { get; set; }

    [BindProperty] public int MotoristaId { get; set; }
    [BindProperty] public int VeiculoId { get; set; }
    [BindProperty] public int LinhaId { get; set; }
    [BindProperty] public int AtribuicaoId { get; set; }
    [BindProperty] public int RotaId { get; set; }
    [BindProperty] public int ViagemId { get; set; }

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");
    private bool IsAdmin => (HttpContext.Session.GetString("roles") ?? "").Contains("Admin");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        if (!IsAdmin) return RedirectToPage("/Index");
        Escala = await _api.GetEscalaAsync(Token);
        Opcoes = await _api.GetEscalaOpcoesAsync(Token);
        return Page();
    }

    public async Task<IActionResult> OnPostAtribuirAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { motoristaId = MotoristaId, veiculoId = VeiculoId, linhaId = LinhaId };
        var res = await _api.AtribuirAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Motorista atribuído ao veículo e à linha.";
        else Erro = res.Erro;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostIniciarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { atribuicaoId = AtribuicaoId, rotaId = RotaId };
        var res = await _api.IniciarViagemAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Viagem iniciada (EmAndamento).";
        else Erro = res.Erro;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConcluirAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { viagemId = ViagemId };
        var res = await _api.ConcluirViagemAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Viagem concluída.";
        else Erro = res.Erro;
        return RedirectToPage();
    }
}
