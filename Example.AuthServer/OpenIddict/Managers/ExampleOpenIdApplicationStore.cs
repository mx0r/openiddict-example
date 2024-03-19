using Example.AuthServer.Domain;
using Example.AuthServer.OpenIddict.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenIddict.EntityFrameworkCore;

namespace Example.AuthServer.OpenIddict.Managers;

[UsedImplicitly]
public class ExampleOpenIdApplicationStore(
    IMemoryCache cache,
    ServerDbContext context,
    IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
    : OpenIddictEntityFrameworkCoreApplicationStore<ExampleOpenIdApplication, ExampleOpenIdAuthorization,
        ExampleOpenIdToken, ServerDbContext, string>(cache, context, options)
{
    public virtual ValueTask<string?> GetPartnerTokenAsync(
        ExampleOpenIdApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);
        return new ValueTask<string?>(application.PartnerToken);
    }

    public virtual ValueTask SetPartnerTokenAsync(
        ExampleOpenIdApplication application, string? partnerToken,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);
        application.PartnerToken = partnerToken;

        return default;
    }
}
