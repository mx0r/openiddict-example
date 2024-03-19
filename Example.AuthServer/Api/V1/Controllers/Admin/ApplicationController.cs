using Example.AuthServer.Api.V1.DataObjects;
using Example.AuthServer.Api.V1.Handlers.Admin.Application;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.AuthServer.Api.V1.Controllers.Admin;

[Route("/api/v1/admin/application")]
public class ApplicationController
    : Controller
{
    [HttpGet]
    [Authorize("admin:auth", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ApplicationDto[]>> ListApplications(
        [FromServices] IMediator mediator,
        [FromQuery] int? count = null, [FromQuery] int? offset = null,
        CancellationToken token = default)
    {
        var response = await mediator.Send(new ListApplicationsRequest(count, offset), token);

        return Ok(response.Output);
    }
}
