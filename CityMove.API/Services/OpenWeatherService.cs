using System.Text.Json;

namespace CityMove.API.Services;

public class OpenWeatherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public OpenWeatherService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<object?> ObterClimaAsync(string cidade)
    {
        var cfg = _config.GetSection("OpenWeather");
        var apiKey = cfg["ApiKey"];
        var baseUrl = cfg["BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("COLOQUE"))
            return new { erro = "OpenWeather API key não configurada em appsettings.json." };

        var url = $"{baseUrl}weather?q={Uri.EscapeDataString(cidade)}&appid={apiKey}&units=metric&lang=pt_br";
        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return new { erro = $"Falha ao consultar clima ({(int)resp.StatusCode}).", cidade };

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        return new
        {
            cidade = root.GetProperty("name").GetString(),
            temperatura = root.GetProperty("main").GetProperty("temp").GetDouble(),
            sensacao = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
            umidade = root.GetProperty("main").GetProperty("humidity").GetInt32(),
            descricao = root.GetProperty("weather")[0].GetProperty("description").GetString(),
            vento = root.GetProperty("wind").GetProperty("speed").GetDouble()
        };
    }
}
