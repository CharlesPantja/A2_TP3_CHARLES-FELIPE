using CityMove.Web.Models;
using CityMove.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityMove.Web.Pages.Admin;

public class MotoristasModel : PageModel
{
    private readonly ApiClient _api;
    public MotoristasModel(ApiClient api) => _api = api;

    public List<MotoristaAdminVm> Motoristas { get; set; } = new();
    public bool Editando => Id is not null && Id != 0;

    [BindProperty] public int? Id { get; set; }
    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Senha { get; set; } = "";
    [BindProperty] public string CNH { get; set; } = "";
    [BindProperty] public string CategoriaCNH { get; set; } = "D";
    [BindProperty] public DateTime ValidadeCNH { get; set; } = DateTime.Today.AddYears(3);
    [BindProperty] public bool Disponivel { get; set; } = true;

    [TempData] public string? Mensagem { get; set; }
    [TempData] public string? Erro { get; set; }

    private string? Token => HttpContext.Session.GetString("token");

    public async Task<IActionResult> OnGetAsync(int? editId)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        Motoristas = await _api.GetMotoristasAdminAsync(Token);

        if (editId is not null)
        {
            var m = Motoristas.FirstOrDefault(x => x.Id == editId);
            if (m is not null)
            {
                Id = m.Id; Nome = m.Nome ?? ""; Email = m.Email ?? "";
                CNH = m.CNH; CategoriaCNH = m.CategoriaCNH;
                ValidadeCNH = m.ValidadeCNH; Disponivel = m.Disponivel;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        // No cadastro (POST) a API exige Nome/Email/Senha; na edição (PUT) usa só os dados do motorista.
        var dto = new
        {
            nome = Nome,
            email = Email,
            senha = string.IsNullOrEmpty(Senha) ? "Motorista@123" : Senha,
            cnh = CNH,
            categoriaCNH = CategoriaCNH,
            validadeCNH = ValidadeCNH,
            disponivel = Disponivel
        };
        var res = await _api.SalvarMotoristaAsync(Token, Id, dto);

        if (res.Sucesso) Mensagem = Editando ? "Motorista atualizado." : "Motorista cadastrado.";
        else Erro = res.Erro;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id)
    {
        if (string.IsNullOrEmpty(Token)) return RedirectToPage("/Login");

        var res = await _api.ExcluirMotoristaAsync(Token, id);
        if (res.Sucesso) Mensagem = "Motorista removido.";
        else Erro = res.Erro;

        return RedirectToPage();
    }
}
