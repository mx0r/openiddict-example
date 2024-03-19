using Example.AuthServer.Api.Handlers.Authorization.ExchangeGrants;
using JetBrains.Annotations;
using MediatR;
using OpenIddict.Abstractions;

namespace Example.AuthServer.Api.Handlers.Authorization;

[UsedImplicitly]
public class ExchangeGrantHandlerRouter(ISender mediator, IServiceScopeFactory scopeFactory)
    : AbstractExchangeGrantHandler<ExchangeGrantRouterRequest>(scopeFactory)
{
    protected override async Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, ExchangeGrantRouterRequest request,
        CancellationToken cancellationToken)
    {
        AbstractExchangeGrantRequest grantRequest;
        var oidRequest = request.OpenIdRequest;
        if (oidRequest.IsClientCredentialsGrantType())
        {
            grantRequest = new ExchangeClientCredentialsGrantRequest(request.HttpContext);
        }
        else if (oidRequest.IsPasswordGrantType())
        {
            grantRequest = new ExchangePasswordGrantRequest(request.HttpContext);
        }
        else if (oidRequest.IsAuthorizationCodeGrantType())
        {
            grantRequest = new ExchangeAuthorizationCodeGrantRequest(request.HttpContext);
        }
        else if (oidRequest.IsRefreshTokenGrantType())
        {
            grantRequest = new ExchangeRefreshTokenGrantRequest(request.HttpContext);
        }
        else
        {
            // when the grant type is not supported
            throw new InvalidOperationException("Given grant type is not supported.");
        }

        // call mediator to handle the request
        var grantResponse = await mediator.Send(grantRequest, cancellationToken);

        // return the response
        return new ExchangeGrantResponse(grantResponse.ClaimsPrincipal);
    }
}

public record ExchangeGrantRouterRequest(HttpContext HttpContext)
    : AbstractExchangeGrantRequest(HttpContext);
