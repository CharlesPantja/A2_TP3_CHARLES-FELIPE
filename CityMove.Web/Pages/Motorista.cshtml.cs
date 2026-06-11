using System.Globalization;
using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages;

public class MotoristaModel : PageModel
{
    private readonly ApiClient _api;
    public MotoristaModel(ApiClient api) => _api = api;

    public ViagemAtualVm? Viagem { get; set; }
    public static readonly string[] Tipos = { "Atraso", "Acidente", "PaneFelec", "PaneMecanica", "Outro" };

    [BindProperty] public int ViagemId { get; set; }
    [BindProperty] public int VeiculoId { get; set; }
    [BindProperty] public int MotoristaId { get; set; }
    // Recebidos como texto (formato internacional, com ponto) e convertidos com InvariantCulture
    [BindProperty] public string Latitude { get; set; } = "";
    [BindProperty] public string Longitude { get; set; } = "";
    [BindProperty] public string Velocidade { get; set; } = "";
    [BindProperty] public string TipoOcorrencia { get; set; } = "Atraso";
    [BindProperty] public string Descricao { get; set; } = "";

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");
    private bool IsMotorista => (HttpContext.Session.GetString("roles") ?? "").Contains("Motorista");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        if (!IsMotorista) return RedirectToPage("/Index");
        Viagem = await _api.GetViagemAtualAsync(Token);
        return Page();
    }

    public async Task<IActionResult> OnPostGpsAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var inv = CultureInfo.InvariantCulture;
        if (!decimal.TryParse(Latitude, NumberStyles.Float, inv, out var lat) ||
            !decimal.TryParse(Longitude, NumberStyles.Float, inv, out var lng))
        {
            Erro = "Coordenadas inválidas. Capture a posição novamente.";
            return RedirectToPage();
        }
        decimal.TryParse(Velocidade, NumberStyles.Float, inv, out var vel);

        var dto = new { veiculoId = VeiculoId, viagemId = ViagemId, latitude = lat, longitude = lng, velocidade = vel };
        var res = await _api.EnviarGpsAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Posição GPS enviada com sucesso!";
        else Erro = res.Erro;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOcorrenciaAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");
        var dto = new { viagemId = ViagemId, motoristaId = MotoristaId, tipoOcorrencia = TipoOcorrencia, descricao = Descricao };
        var res = await _api.RegistrarOcorrenciaAsync(Token, dto);
        if (res.Sucesso) Mensagem = "Ocorrência registrada.";
        else Erro = res.Erro;
        return RedirectToPage();
    }
}
