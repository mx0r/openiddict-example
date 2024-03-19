using Example.AuthServer.Api.V1.DataObjects;
using Example.AuthServer.OpenIddict.Entities;
using Example.AuthServer.OpenIddict.Managers;
using JetBrains.Annotations;
using MediatR;
using OpenIddict.Core;

namespace Example.AuthServer.Api.V1.Handlers.Admin.Application;

[UsedImplicitly]
public class ListApplicationsHandler(IServiceScopeFactory scopeFactory)
    : IRequestHandler<ListApplicationsRequest, ListApplicationResponse>
{
    public async Task<ListApplicationResponse> Handle(
        ListApplicationsRequest request,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var manager = scope.ServiceProvider
                .GetRequiredService<OpenIddictApplicationManager<ExampleOpenIdApplication>>()
            as ExampleOpenIdApplicationManager;

        var result = new List<ApplicationDto>();
        await foreach (var app in manager!.ListAsync(request.Count, request.Offset, cancellationToken))
        {
            result.Add(new ApplicationDto
            {
                ClientId = app.ClientId!,
                PartnerToken = app.PartnerToken,
                DisplayName = app.DisplayName,
                RedirectUris = app.RedirectUris,
                Permissions = app.Permissions,
            });
        }

        return new ListApplicationResponse(result.ToArray());
    }
}

public record ListApplicationsRequest(int? Count = null, int? Offset = null)
    : IRequest<ListApplicationResponse>;

public record ListApplicationResponse(ApplicationDto[] Output);
