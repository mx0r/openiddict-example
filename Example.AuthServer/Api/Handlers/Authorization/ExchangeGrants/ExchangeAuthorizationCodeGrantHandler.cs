using System.Security.Claims;
using Example.AuthServer.Domain.DataSources;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Example.AuthServer.Api.Extensions;

namespace Example.AuthServer.Api.Handlers.Authorization.ExchangeGrants;

[UsedImplicitly]
public class ExchangeAuthorizationCodeGrantHandler(IServiceScopeFactory scopeFactory)
    : AbstractExchangeGrantHandler<ExchangeAuthorizationCodeGrantRequest>(scopeFactory)
{
    protected override async Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, ExchangeAuthorizationCodeGrantRequest request,
        CancellationToken cancellationToken)
    {
        var oidRequest = request.OpenIdRequest;
        var httpContext = request.HttpContext;

        // retrieve the claims principal stored in the authorization code/refresh token.
        var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result.Principal?.GetClaim("sub") is not { } customerUrn
            || oidRequest.ClientId is null)
        {
            // when user is not authenticated correctly, throw an exception
            throw new InvalidOperationException("The OID request or user principal is not valid.");
        }

        var dsCustomer = sp.GetRequiredService<ICustomerDataSource>();
        var customer = await dsCustomer.RequireByUrnAsync(customerUrn, cancellationToken);

        // create claims principal for the identity
        var claimsIdentity = customer.ToClaimsIdentity();

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

        // return claims principal for this identity in the response
        return new ExchangeGrantResponse(new ClaimsPrincipal(claimsIdentity));
    }
}

public record ExchangeAuthorizationCodeGrantRequest(HttpContext HttpContext)
    : AbstractExchangeGrantRequest(HttpContext);
