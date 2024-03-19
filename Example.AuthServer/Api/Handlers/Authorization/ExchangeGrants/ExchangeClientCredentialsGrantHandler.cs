using System.Security.Claims;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.OpenIddict.Entities;
using JetBrains.Annotations;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;
using Example.AuthServer.Api.Extensions;

namespace Example.AuthServer.Api.Handlers.Authorization.ExchangeGrants;

[UsedImplicitly]
public class ExchangeClientCredentialsGrantHandler(IServiceScopeFactory scopeFactory)
    : AbstractExchangeGrantHandler<ExchangeClientCredentialsGrantRequest>(scopeFactory)
{
    protected override async Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, ExchangeClientCredentialsGrantRequest request,
        CancellationToken cancellationToken)
    {
        var oidRequest = request.OpenIdRequest;

        // retrieve partner token from openid application
        var clientId = oidRequest.ClientId ?? throw new InvalidOperationException("Missing Client ID");
        var applicationManager = sp.GetRequiredService<OpenIddictApplicationManager<ExampleOpenIdApplication>>();
        var application = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        if (application is null)
        {
            // when the application is not available, throw an exception
            throw new InvalidOperationException("The application is available");
        }

        // create claims identity for the client
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (application.PartnerToken is { } partnerToken)
        {
            // retrieve partner information by token
            var partnerDataSource = sp.GetRequiredService<IPartnerDataSource>();
            var partner = await partnerDataSource.RequireByTokenAsync(partnerToken, cancellationToken);

            // add claims for the partner
            identity.AddClaim(OpenIddictConstants.Claims.Subject, partner.Urn);
        }
        else
        {
            // add pure client claim with client URN as subject
            identity.AddClaim(OpenIddictConstants.Claims.Subject, $"client:{application.ClientId}");
        }

        // add resources for given scopes
        var resources = await oidRequest.ToResources(sp, cancellationToken); 
        identity.SetResources(resources);

        // add destinations (scopes) for the claims; all except "secret_value" are allowed
        identity.SetDestinations(static claim => claim.Type switch
        {
            "secret_value" => Array.Empty<string>(),
            _ => [
                OpenIddictConstants.Destinations.AccessToken, 
                OpenIddictConstants.Destinations.IdentityToken
            ]
        });

        // create claims principal for the identity
        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(oidRequest.GetScopes());

        return new ExchangeGrantResponse(claimsPrincipal);
    }
}

public record ExchangeClientCredentialsGrantRequest(HttpContext HttpContext)
    : AbstractExchangeGrantRequest(HttpContext);
