using System.Text.Json;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Weather;

public sealed class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleWeatherOptions _options;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IOptions<GoogleWeatherOptions> options,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherConditionsResponse?> GetCurrentConditionsAsync(
        double lat,
        double lng,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = BuildUrl(
                "currentConditions:lookup",
                $"location.latitude={Invariant(lat)}&location.longitude={Invariant(lng)}&languageCode=es&unitsSystem=METRIC");

            if (await GetJsonAsync(url, cancellationToken) is not { } json)
            {
                return null;
            }

            return new WeatherConditionsResponse
            {
                Condition = GetString(json, "weatherCondition", "type") ?? string.Empty,
                ConditionDescription = GetText(json, "weatherCondition", "description"),
                IconBaseUri = GetString(json, "weatherCondition", "iconBaseUri") ?? string.Empty,
                TemperatureC = GetDouble(json, "temperature", "degrees"),
                FeelsLikeC = GetDouble(json, "feelsLikeTemperature", "degrees"),
                RelativeHumidity = GetInt(json, "relativeHumidity"),
                WindKmh = GetDouble(json, "wind", "speed", "value"),
                GustKmh = GetDouble(json, "wind", "gust", "value"),
                PrecipitationProbability = GetInt(json, "precipitation", "probability", "percent"),
                ThunderstormProbability = GetInt(json, "thunderstormProbability"),
                CloudCover = GetInt(json, "cloudCover"),
                UvIndex = GetInt(json, "uvIndex"),
                IsDaytime = GetBool(json, "isDaytime"),
                TimeZone = GetString(json, "timeZone", "id") ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Weather API (currentConditions)");
            return null;
        }
    }

    public async Task<List<WeatherAlertResponse>?> GetWeatherAlertsAsync(
        double lat,
        double lng,
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lang = string.IsNullOrWhiteSpace(languageCode) ? "es" : languageCode;
            var url = BuildUrl(
                "publicAlerts:lookup",
                $"location.latitude={Invariant(lat)}&location.longitude={Invariant(lng)}&languageCode={lang}");

            if (await GetJsonAsync(url, cancellationToken) is not { } json ||
                !json.TryGetProperty("alerts", out var alerts))
            {
                return [];
            }

            var result = new List<WeatherAlertResponse>();
            foreach (var alert in alerts.EnumerateArray())
            {
                result.Add(new WeatherAlertResponse
                {
                    AlertTitle = GetString(alert, "alertTitle") ?? string.Empty,
                    EventType = GetString(alert, "eventType") ?? string.Empty,
                    Severity = GetString(alert, "severity") ?? string.Empty,
                    Description = GetString(alert, "description"),
                    Instruction = GetString(alert, "instruction"),
                    AreaName = GetString(alert, "areaName"),
                    StartTime = GetDateTime(alert, "startTime"),
                    ExpirationTime = GetDateTime(alert, "expirationTime"),
                    DataSource = GetString(alert, "dataSource", "publisher")
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Weather API (publicAlerts)");
            return null;
        }
    }

    private async Task<JsonElement?> GetJsonAsync(string url, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google Weather API returned status {StatusCode}",
                response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }

    private string BuildUrl(string endpoint, string query)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/{endpoint}?{query}&key={Uri.EscapeDataString(_options.ApiKey)}";
    }

    private static string? GetString(JsonElement element, params string[] path)
        => GetElement(element, path) is { } child && child.ValueKind == JsonValueKind.String
            ? child.GetString()
            : null;

    private static string? GetText(JsonElement element, params string[] path)
        => GetElement(element, path) is { } child
            ? (GetString(child, "text") ?? string.Empty)
            : null;

    private static int GetInt(JsonElement element, params string[] path)
        => GetElement(element, path) is { } child && child.TryGetInt32(out var value) ? value : 0;

    private static double GetDouble(JsonElement element, params string[] path)
        => GetElement(element, path) is { } child && child.TryGetDouble(out var value) ? value : 0d;

    private static bool GetBool(JsonElement element, params string[] path)
        => GetElement(element, path) is { } child && child.ValueKind == JsonValueKind.True;

    private static DateTimeOffset? GetDateTime(JsonElement element, params string[] path)
    {
        var child = GetElement(element, path);
        if (child is null)
        {
            return null;
        }

        var raw = child.ToString();
        return DateTimeOffset.TryParse(raw, out var parsed) ? parsed : null;
    }

    private static JsonElement? GetElement(JsonElement element, string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.ValueKind == JsonValueKind.Null ? null : current;
    }

    private static string Invariant(double value)
        => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}