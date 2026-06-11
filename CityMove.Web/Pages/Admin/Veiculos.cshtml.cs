using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Admin;

public class VeiculosModel : PageModel
{
    private readonly ApiClient _api;
    public VeiculosModel(ApiClient api) => _api = api;

    public List<VeiculoAdminVm> Veiculos { get; set; } = new();
    public List<LinhaAdminVm> Linhas { get; set; } = new();
    public bool Editando => Id is not null && Id != 0;

    [BindProperty] public int? Id { get; set; }
    [BindProperty] public int LinhaId { get; set; }
    [BindProperty] public string Placa { get; set; } = "";
    [BindProperty] public string Modelo { get; set; } = "";
    [BindProperty] public string Marca { get; set; } = "";
    [BindProperty] public int Capacidade { get; set; } = 40;
    [BindProperty] public string StatusVeiculo { get; set; } = "Ativo";

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    public static readonly string[] Status = { "Ativo", "Manutencao", "Inativo" };

    private string? Token => HttpContext.Session.GetString("token");

    public async Task<IActionResult> OnGetAsync(int? editId)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        Veiculos = await _api.GetVeiculosAdminAsync(Token);
        Linhas = await _api.GetLinhasAdminAsync(Token);

        if (editId is not null)
        {
            var v = Veiculos.FirstOrDefault(x => x.Id == editId);
            if (v is not null)
            {
                Id = v.Id; LinhaId = v.LinhaId; Placa = v.Placa; Modelo = v.Modelo;
                Marca = v.Marca; Capacidade = v.Capacidade; StatusVeiculo = v.StatusVeiculo;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var dto = new { linhaId = LinhaId, placa = Placa, modelo = Modelo, marca = Marca, capacidade = Capacidade, statusVeiculo = StatusVeiculo };
        var res = await _api.SalvarVeiculoAsync(Token, Id, dto);

        if (res.Sucesso) Mensagem = Editando ? "Veículo atualizado." : "Veículo cadastrado.";
        else Erro = res.Erro;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var res = await _api.ExcluirVeiculoAsync(Token, id);
        if (res.Sucesso) Mensagem = "Veículo removido.";
        else Erro = res.Erro;

        return RedirectToPage();
    }
}
