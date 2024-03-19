using Example.WebApi.Api.DataObjects;
using Example.WebApi.Api.Handlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.WebApi.Api.Controllers;

[ApiController]
[Route("/api/v1/weather-forecast")]
public class WeatherForecastController
    : ControllerBase
{
    [HttpGet]
    [Authorize("read:weather")]
    public async Task<ActionResult<WeatherForecast[]>> GetWeatherForecastAsync(
        [FromServices] IMediator mediator,
        [FromQuery(Name = "numberDays")] int numberDays = 5,
        CancellationToken token = default)
    {
        var request = new GetWeatherForecastRequest(numberDays);
        var response = await mediator.Send(request, token);

        return Ok(response.Output);
    }

    [HttpGet("one-day")]
    [Authorize("read:weather")]
    public async Task<ActionResult<WeatherForecast[]>> GetWeatherForecastOneDayAsync(
        [FromServices] IMediator mediator,
        CancellationToken token = default)
    {
        var request = new GetWeatherForecastRequest(1);
        var response = await mediator.Send(request, token);

        return Ok(response.Output);
    }

    [HttpPost]
    [Authorize("write:weather")]
    public ActionResult PostWeatherForecast()
    {
        // just a dummy text
        return Ok("Weather forecast created successfully.");
    }
}
