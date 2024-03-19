using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;

namespace Example.AuthServer.Api.Handlers.Authorization;

public abstract class AbstractExchangeGrantHandler<TRequest>(IServiceScopeFactory scopeFactory)
    : IRequestHandler<TRequest, ExchangeGrantResponse>
    where TRequest : AbstractExchangeGrantRequest
{
    protected abstract Task<ExchangeGrantResponse> HandleInternal(
        IServiceProvider sp, TRequest request, CancellationToken cancellationToken);

    public async Task<ExchangeGrantResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        using var sp = scopeFactory.CreateScope();
        return await HandleInternal(sp.ServiceProvider, request, cancellationToken);
    }
}

public abstract record AbstractExchangeGrantRequest(HttpContext HttpContext)
    : IRequest<ExchangeGrantResponse>
{
    public OpenIddictRequest OpenIdRequest
        => HttpContext.GetOpenIddictServerRequest()
           ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
}

public record ExchangeGrantResponse(ClaimsPrincipal ClaimsPrincipal);