using Example.AuthServer.Api.Handlers.Authorization;
using Example.AuthServer.Api.Handlers.Exceptions;
using Example.AuthServer.Attributes;
using Example.AuthServer.Controllers;
using Example.AuthServer.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;

namespace Example.AuthServer.Api.Controllers;

public class AuthorizationController
    : Controller
{
    [HttpPost("~/oauth/token")]
    public async Task<IActionResult> Exchange([FromServices] IMediator mediator)
    {
        try
        {
            // call the exchange grant handler router and get the response
            var response = await mediator.Send(new ExchangeGrantRouterRequest(HttpContext));

            // return claims principal as a result of the token exchange
            return SignIn(response.ClaimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        catch (AuthenticationForbiddenException agEx)
        {
            return new ForbidResult(agEx.Properties);
        }
    }

    [HttpGet("~/oauth/authorize")]
    [HttpPost("~/oauth/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize([FromServices] IMediator mediator)
    {
        try
        {
            var response = await mediator.Send(new AuthorizeRequest(HttpContext));
            return response switch
            {
                ChallengeAuthorizeResponse challengeResponse
                    => RedirectToAction(nameof(AccountController.Login), "Account",
                        new
                        {
                            challengeResponse.ClientId,
                            ReturnUrl = challengeResponse.RedirectUri,
                        }),

                ConsentAuthorizeResponse consentResponse
                    => View(new AuthorizeViewModel
                    {
                        ClientId = consentResponse.ClientId,
                        ReturnUrl = consentResponse.RedirectUri,
                        ApplicationName = consentResponse.ApplicationName,
                        UserName = consentResponse.UserName,
                        Scopes = consentResponse.Scopes.Select(kvp => new ScopeViewModel
                        {
                            Name = kvp.Key, Description = kvp.Value
                        })
                    }),

                SignInAuthorizeResponse signInResponse
                    => SignIn(signInResponse.ClaimsPrincipal, signInResponse.AuthenticationScheme),

                _ => BadRequest()
            };
        }
        catch (AuthenticationForbiddenException agEx)
        {
            return new ForbidResult(agEx.Properties);
        }
    }

    [Authorize, FormValueRequired("submit.Allow")]
    [HttpPost("~/oauth/authorize"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Allow(
        [FromServices] IMediator mediator,
        CancellationToken token = default)
    {
        try
        {
            var response = await mediator.Send(new AllowConsentRequest(HttpContext), token);
            return SignIn(response.ClaimsPrincipal, response.AuthenticationScheme);
        }
        catch (AuthenticationForbiddenException agEx)
        {
            return new ForbidResult(agEx.Properties);
        }
    }

    [Authorize, FormValueRequired("submit.Deny")]
    [HttpPost("~/oauth/authorize"), ValidateAntiForgeryToken]
    public IActionResult Deny()
        => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
}
