using System.Security.Claims;
using Example.AuthServer.Api.Handlers.Exceptions;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.OpenIddict.Entities;
using Example.AuthServer.OpenIddict.Managers;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;
using Example.AuthServer.Api.Extensions;
using Example.AuthServer.Extensions;

namespace Example.AuthServer.Api.Handlers.Authorization;

[UsedImplicitly]
public class AuthorizeRequestHandler(IServiceScopeFactory scopeFactory)
    : IRequestHandler<AuthorizeRequest, AuthorizeResponse>
{
    public async Task<AuthorizeResponse> Handle(
        AuthorizeRequest request, CancellationToken cancellationToken)
    {
        var httpContext = request.HttpContext;

        // retrieve openid request from the http context
        var oidRequest = httpContext.GetOpenIddictServerRequest()
                         ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // authenticate the request
        var result = await httpContext.AuthenticateAsync();

        if (!result.Succeeded
            || oidRequest.HasPrompt(OpenIddictConstants.Prompts.Login)
            || (oidRequest.MaxAge != null
                && result.Properties?.IssuedUtc != null
                && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(oidRequest.MaxAge.Value)))
        {
            // if authentication failed, or login is always requested or max age is exceeded,...
            if (oidRequest.HasPrompt(OpenIddictConstants.Prompts.None))
            {
                // when no prompt was requested and authentication was not successful,
                // there is no way to sign in via this flow and authentication failed
                throw new AuthenticationForbiddenException(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                });
            }

            // prepare return URL, that will point back to the authorize endpoint
            var returnUrl = httpContext.Request.PathBase
                            + httpContext.Request.Path
                            + httpContext.Request.QueryString;

            // redirect to the login form with appropriate parameters
            return new ChallengeAuthorizeResponse(oidRequest.ClientId!, returnUrl);
        }

        if (result.Principal?.GetClaim(ClaimTypes.NameIdentifier) is not { } customerUrn
            || oidRequest.ClientId is null)
        {
            // when user is not authenticated correctly, throw an exception
            throw new InvalidOperationException("The OID request or user principal is not valid.");
        }

        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        // retrieve information about the application
        var applicationManager = sp.GetRequiredService<ExampleOpenIdApplicationManager>();
        var clientApplication = await applicationManager.FindByClientIdAsync(oidRequest.ClientId, cancellationToken);
        if (clientApplication is null)
        {
            // when client application is not found, throw an exception
            throw new InvalidOperationException("The client application is not found.");
        }

        // retrieve information about the logged user
        var dsCustomer = sp.GetRequiredService<ICustomerDataSource>();
        var customer = await dsCustomer.RequireByUrnAsync(customerUrn, cancellationToken);

        // retrieve possible permanent authorizations
        var authorizationManager = sp.GetRequiredService<OpenIddictAuthorizationManager<ExampleOpenIdAuthorization>>();
        var authorizations = await authorizationManager.FindAsync(
                subject: customerUrn,
                client: oidRequest.ClientId,
                status: OpenIddictConstants.Statuses.Valid,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                cancellationToken: cancellationToken)
            .ToListAsync();

        var consentType = await applicationManager.GetConsentTypeAsync(clientApplication, cancellationToken);
        switch (consentType)
        {
            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.Explicit
                when authorizations.Count is not 0
                     && !oidRequest.HasPrompt(OpenIddictConstants.Prompts.Consent):

                // create claims identity for given customer
                var claimsIdentity = customer.ToClaimsIdentity();

                // for now, all requested scopes are granted
                claimsIdentity.SetScopes(oidRequest.GetScopes());

                // add resources for given scopes
                var resources = await oidRequest.ToResources(sp, cancellationToken);
                claimsIdentity.SetResources(resources);

                // create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes
                var authorization = authorizations.LastOrDefault();
                authorization ??= await authorizationManager.CreateAsync(
                    identity: claimsIdentity,
                    subject: customerUrn,
                    client: oidRequest.ClientId,
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: claimsIdentity.GetScopes(),
                    cancellationToken: cancellationToken);

                var authorizationId = await authorizationManager.GetIdAsync(authorization, cancellationToken);
                claimsIdentity.SetAuthorizationId(authorizationId);

                // add destinations (scopes) for the claims; all except "secret_value" are allowed
                claimsIdentity.SetDestinations(static claim => claim.Type switch
                {
                    "secret_value" => Array.Empty<string>(),
                    _ =>
                    [
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    ]
                });

                return new SignInAuthorizeResponse(
                    oidRequest.ClientId, new ClaimsPrincipal(claimsIdentity),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case OpenIddictConstants.ConsentTypes.Explicit
                when oidRequest.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic
                when oidRequest.HasPrompt(OpenIddictConstants.Prompts.None):
                throw new AuthenticationForbiddenException(
                    new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    });

            // In every other case, render the consent form.
            default:
                var scopeManager = sp.GetRequiredService<OpenIddictScopeManager<ExampleOpenIdScope>>();
                var requestScopes = await scopeManager
                    .FindByNamesAsync(oidRequest.GetScopes(), cancellationToken)
                    .ToListAsync();

                return new ConsentAuthorizeResponse(
                    oidRequest.ClientId,
                    clientApplication.DisplayName ?? "Application", 
                    customer.FullName,
                    requestScopes
                        .Where(s => s.Name is not null)
                        .ToDictionary(kvp => kvp.Name!, kvp => kvp.DisplayName ?? kvp.Name!));
        }
    }
}

public record AuthorizeRequest(HttpContext HttpContext)
    : IRequest<AuthorizeResponse>;

public abstract record AuthorizeResponse(AuthorizeResponses Type, string ClientId);

public record ChallengeAuthorizeResponse(string ClientId, string? RedirectUri = null)
    : AuthorizeResponse(AuthorizeResponses.Challenge, ClientId);

public record ConsentAuthorizeResponse(
    string ClientId,
    string ApplicationName,
    string UserName,
    Dictionary<string, string> Scopes,
    string? RedirectUri = null)
    : AuthorizeResponse(AuthorizeResponses.Consent, ClientId);

public record SignInAuthorizeResponse(string ClientId, ClaimsPrincipal ClaimsPrincipal, string AuthenticationScheme)
    : AuthorizeResponse(AuthorizeResponses.SignIn, ClientId);

public enum AuthorizeResponses
{
    Challenge,
    Consent,
    SignIn,
}
