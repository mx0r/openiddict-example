using System.Security.Claims;
using JetBrains.Annotations;
using OpenIddict.Abstractions;
using Example.AuthServer.Api.Extensions;

namespace Example.AuthServer.Api.Handlers.Authorization.ExchangeGrants;

[UsedImplicitly]
public class ExchangePasswordGrantHandler(IServiceScopeFactory scopeFactory)
    : AbstractExchangeGrantHandler<ExchangePasswordGrantRequest>(scopeFactory)
{
    protected override async Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, ExchangePasswordGrantRequest request,
        CancellationToken cancellationToken)
    {
        var oidRequest = request.OpenIdRequest;
        var customer = await oidRequest.RequireValidCustomerAsync(sp, cancellationToken);

        // create claims principal for the identity
        var identity = customer.ToClaimsIdentity();
        
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

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(oidRequest.GetScopes());

        return new ExchangeGrantResponse(claimsPrincipal);
    }
}

public record ExchangePasswordGrantRequest(HttpContext HttpContext)
    : AbstractExchangeGrantRequest(HttpContext);
