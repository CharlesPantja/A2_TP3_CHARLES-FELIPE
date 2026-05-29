using System.Text.Json;

namespace CityMove.API.Services;

public class NominatimService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public NominatimService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    /// <summary>Geocodifica um endereço em latitude/longitude usando OpenStreetMap/Nominatim.</summary>
    public async Task<object?> GeocodificarAsync(string endereco)
    {
        var baseUrl = _config["Nominatim:BaseUrl"];
        var ua = _config["Nominatim:UserAgent"] ?? "CityMove/1.0";

        var url = $"{baseUrl}search?q={Uri.EscapeDataString(endereco)}&format=json&limit=1";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("User-Agent", ua);

        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
            return new { erro = $"Falha ao geocodificar ({(int)resp.StatusCode}).", endereco };

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        if (doc.RootElement.GetArrayLength() == 0)
            return new { erro = "Endereço não encontrado.", endereco };

        var item = doc.RootElement[0];
        return new
        {
            endereco,
            nomeEncontrado = item.GetProperty("display_name").GetString(),
            latitude = decimal.Parse(item.GetProperty("lat").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
            longitude = decimal.Parse(item.GetProperty("lon").GetString()!, System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
