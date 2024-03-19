using System.Text.Json.Serialization;

namespace Example.WebApi.Api.DataObjects;

public record WeatherForecast
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }

    [JsonPropertyName("temperatureCelsius")]
    public int TemperatureC { get; init; }

    [JsonPropertyName("temperatureFahrenheit")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    public WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        this.TemperatureC = TemperatureC;
        this.Summary = Summary;
        this.Date = Date;
    }
}
