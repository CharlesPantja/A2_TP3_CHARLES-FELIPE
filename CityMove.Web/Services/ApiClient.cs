using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CityMove.Web.Models;

namespace CityMove.Web.Services;

/// <summary>Cliente HTTP tipado que consome a CityMove.API.</summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(HttpClient http) => _http = http;

    // ---------- Endpoints públicos ----------
    public async Task<List<LinhaVm>> GetLinhasAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<LinhaVm>>("api/public/linhas", JsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<HorarioVm>> GetHorariosAsync(int linhaId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<HorarioVm>>($"api/public/linhas/{linhaId}/horarios", JsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<ParadaVm>> GetParadasAsync(int linhaId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ParadaVm>>($"api/public/linhas/{linhaId}/paradas", JsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<ClimaVm> GetClimaAsync(string cidade)
    {
        try
        {
            var resp = await _http.GetAsync($"api/public/clima/{Uri.EscapeDataString(cidade)}");
            var json = await resp.Content.ReadAsStringAsync();
            var c = JsonSerializer.Deserialize<ClimaVm>(json, JsonOpts);
            return c ?? new ClimaVm(cidade, null, null, null, null, null, "Resposta inválida da API.");
        }
        catch (Exception ex)
        {
            return new ClimaVm(cidade, null, null, null, null, null, $"Falha ao consultar clima: {ex.Message}");
        }
    }

    public async Task<List<PosicaoVm>> GetPosicoesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<PosicaoVm>>("api/public/veiculos/posicoes", JsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    // ---------- Autenticação ----------
    public async Task<LoginResultVm> LoginAsync(string email, string senha)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", new { email, senha });
            if (!resp.IsSuccessStatusCode)
                return new LoginResultVm(false, null, null, null, "Credenciais inválidas.");

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var token = root.GetProperty("token").GetString();
            var nome = root.TryGetProperty("nome", out var n) ? n.GetString() : null;
            var roles = root.TryGetProperty("roles", out var r)
                ? r.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : new List<string>();
            return new LoginResultVm(true, token, nome, roles, null);
        }
        catch (Exception ex)
        {
            return new LoginResultVm(false, null, null, null, $"Erro de conexão com a API: {ex.Message}");
        }
    }

    // ---------- Endpoint autenticado (Admin) ----------
    public async Task<JsonElement?> GetRelatoriosAsync(string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "api/admin/relatorios");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    // ---------- Helpers autenticados ----------
    private async Task<List<T>> GetAuthListAsync<T>(string endpoint, string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return new();
            return JsonSerializer.Deserialize<List<T>>(await resp.Content.ReadAsStringAsync(), JsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private async Task<ApiResult> SendAuthAsync(HttpMethod method, string endpoint, object? body, string token)
    {
        try
        {
            using var req = new HttpRequestMessage(method, endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body is not null)
                req.Content = JsonContent.Create(body);

            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
                return new ApiResult(true, null);

            var corpo = await resp.Content.ReadAsStringAsync();
            return new ApiResult(false, $"Erro {(int)resp.StatusCode}: {corpo}");
        }
        catch (Exception ex)
        {
            return new ApiResult(false, $"Falha de conexão: {ex.Message}");
        }
    }

    // ---------- CRUD Linhas ----------
    public Task<List<LinhaAdminVm>> GetLinhasAdminAsync(string token)
        => GetAuthListAsync<LinhaAdminVm>("api/linhas", token);

    public Task<ApiResult> SalvarLinhaAsync(string token, int? id, object dto)
        => id is null or 0
            ? SendAuthAsync(HttpMethod.Post, "api/linhas", dto, token)
            : SendAuthAsync(HttpMethod.Put, $"api/linhas/{id}", dto, token);

    public Task<ApiResult> ExcluirLinhaAsync(string token, int id)
        => SendAuthAsync(HttpMethod.Delete, $"api/linhas/{id}", null, token);

    // ---------- CRUD Rotas ----------
    public Task<List<RotaAdminVm>> GetRotasAdminAsync(string token)
        => GetAuthListAsync<RotaAdminVm>("api/rotas", token);

    public Task<ApiResult> SalvarRotaAsync(string token, int? id, object dto)
        => id is null or 0
            ? SendAuthAsync(HttpMethod.Post, "api/rotas", dto, token)
            : SendAuthAsync(HttpMethod.Put, $"api/rotas/{id}", dto, token);

    public Task<ApiResult> ExcluirRotaAsync(string token, int id)
        => SendAuthAsync(HttpMethod.Delete, $"api/rotas/{id}", null, token);

    // ---------- CRUD Veículos ----------
    public Task<List<VeiculoAdminVm>> GetVeiculosAdminAsync(string token)
        => GetAuthListAsync<VeiculoAdminVm>("api/veiculos", token);

    public Task<ApiResult> SalvarVeiculoAsync(string token, int? id, object dto)
        => id is null or 0
            ? SendAuthAsync(HttpMethod.Post, "api/veiculos", dto, token)
            : SendAuthAsync(HttpMethod.Put, $"api/veiculos/{id}", dto, token);

    public Task<ApiResult> ExcluirVeiculoAsync(string token, int id)
        => SendAuthAsync(HttpMethod.Delete, $"api/veiculos/{id}", null, token);

    // ---------- CRUD Motoristas ----------
    public Task<List<MotoristaAdminVm>> GetMotoristasAdminAsync(string token)
        => GetAuthListAsync<MotoristaAdminVm>("api/motoristas", token);

    public Task<ApiResult> SalvarMotoristaAsync(string token, int? id, object dto)
        => id is null or 0
            ? SendAuthAsync(HttpMethod.Post, "api/motoristas", dto, token)
            : SendAuthAsync(HttpMethod.Put, $"api/motoristas/{id}", dto, token);

    public Task<ApiResult> ExcluirMotoristaAsync(string token, int id)
        => SendAuthAsync(HttpMethod.Delete, $"api/motoristas/{id}", null, token);
}
