using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server.AspNetCore;

namespace Example.AuthServer.Api.Handlers.Authorization.ExchangeGrants;

[UsedImplicitly]
public class ExchangeRefreshTokenGrantHandler(IServiceScopeFactory scopeFactory)
    : AbstractExchangeGrantHandler<ExchangeRefreshTokenGrantRequest>(scopeFactory)
{
    protected override async Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, ExchangeRefreshTokenGrantRequest request,
        CancellationToken cancellationToken)
    {
        var authResult = await request.HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (authResult.Principal is null)
        {
            // when the authorization code is not valid
            throw new InvalidOperationException("The authorization code is not valid, principal is NULL.");
        }

        return new ExchangeGrantResponse(authResult.Principal);
    }
}

public record ExchangeRefreshTokenGrantRequest(HttpContext HttpContext)
    : AbstractExchangeGrantRequest(HttpContext);
