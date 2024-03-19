using Example.WebApi.Api.DataObjects;
using MediatR;

namespace Example.WebApi.Api.Handlers;

public class GetWeatherForecastHandler
    : IRequestHandler<GetWeatherForecastRequest, GetWeatherForecastResponse>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<GetWeatherForecastResponse> Handle(
        GetWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var forecasts = Enumerable.Range(1, request.NumberDays)
            .Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
            .ToArray();

        return Task.FromResult(new GetWeatherForecastResponse(forecasts));
    }
}

public record GetWeatherForecastRequest(int NumberDays = 5)
    : IRequest<GetWeatherForecastResponse>;

public record GetWeatherForecastResponse(WeatherForecast[] Output);
