using System.Security.Claims;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.OpenIddict.Entities;
using Example.AuthServer.OpenIddict.Managers;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;
using Example.AuthServer.Api.Extensions;
using Example.AuthServer.Extensions;

namespace Example.AuthServer.Api.Handlers.Authorization;

[UsedImplicitly]
public class AllowConsentRequestHandler(IServiceScopeFactory scopeFactory)
    : IRequestHandler<AllowConsentRequest, AllowConsentResponse>
{
    public async Task<AllowConsentResponse> Handle(AllowConsentRequest request, CancellationToken cancellationToken)
    {
        var httpContext = request.HttpContext;
        var user = httpContext.User;

        // retrieve openid request from the http context
        var oidRequest = httpContext.GetOpenIddictServerRequest()
                         ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        // retrieve information about the application
        var applicationManager = sp.GetRequiredService<ExampleOpenIdApplicationManager>();
        var clientApplication = await applicationManager.FindByClientIdAsync(oidRequest.ClientId!, cancellationToken);
        if (clientApplication is null)
        {
            // when client application is not found, throw an exception
            throw new InvalidOperationException("The client application is not found.");
        }

        if (user.GetClaim(ClaimTypes.NameIdentifier) is not { } customerUrn
            || oidRequest.ClientId is null)
        {
            // when user is not authenticated correctly, throw an exception
            throw new InvalidOperationException("The OID request or user principal is not valid.");
        }

        // retrieve information about the logged user
        var dsCustomer = sp.GetRequiredService<ICustomerDataSource>();
        var customer = await dsCustomer.RequireByUrnAsync(customerUrn, cancellationToken);

        // retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizationManager = sp.GetRequiredService<OpenIddictAuthorizationManager<ExampleOpenIdAuthorization>>();
        var authorizations = await authorizationManager.FindAsync(
                subject: customerUrn,
                client: oidRequest.ClientId,
                status: OpenIddictConstants.Statuses.Valid,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: oidRequest.GetScopes(), 
                cancellationToken: cancellationToken)
            .ToListAsync();

        // create claims identity for given customer
        var claimsIdentity = customer.ToClaimsIdentity();

        // for now, all requested scopes are granted
        claimsIdentity.SetScopes(oidRequest.GetScopes());

        // add resources for given scopes
        var resources = await oidRequest.ToResources(sp, cancellationToken);
        claimsIdentity.SetResources(resources);

        // create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
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
        
        return new AllowConsentResponse(
            new ClaimsPrincipal(claimsIdentity),
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}

public record AllowConsentRequest(HttpContext HttpContext)
    : IRequest<AllowConsentResponse>;

public record AllowConsentResponse(ClaimsPrincipal ClaimsPrincipal, string AuthenticationScheme);
