using Example.AuthServer.OpenIddict.Descriptors;
using Example.AuthServer.OpenIddict.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace Example.AuthServer.OpenIddict.Managers;

[UsedImplicitly]
public class ExampleOpenIdApplicationManager(
    IOpenIddictApplicationCache<ExampleOpenIdApplication> cache,
    ILogger<OpenIddictApplicationManager<ExampleOpenIdApplication>> logger,
    IOptionsMonitor<OpenIddictCoreOptions> options,
    IOpenIddictApplicationStoreResolver resolver)
    : OpenIddictApplicationManager<ExampleOpenIdApplication>(cache, logger, options, resolver)
{
    public override async ValueTask PopulateAsync(
        ExampleOpenIdApplication application, OpenIddictApplicationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        await base.PopulateAsync(application, descriptor, cancellationToken);

        // if the store is our custom application store, set the partner token
        if (Store is ExampleOpenIdApplicationStore applicationStore
            && descriptor is ExampleOpenIdApplicationDescriptor applicationDescriptor)
        {
            await applicationStore.SetPartnerTokenAsync(
                application, applicationDescriptor.PartnerToken, cancellationToken);
        }
    }

    public override async ValueTask PopulateAsync(OpenIddictApplicationDescriptor descriptor,
        ExampleOpenIdApplication application,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await base.PopulateAsync(descriptor, application, cancellationToken);

        if (descriptor is ExampleOpenIdApplicationDescriptor applicationDescriptor
            && Store is ExampleOpenIdApplicationStore applicationStore)
        {
            applicationDescriptor.PartnerToken = await applicationStore
                .GetPartnerTokenAsync(application, cancellationToken);
        }
    }
    
    public virtual ValueTask<string?> GetPartnerTokenAsync(
        ExampleOpenIdApplication application, CancellationToken cancellationToken = default)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (Store is not ExampleOpenIdApplicationStore applicationStore)
        {
            // this should not happen, but we need to be sure
            throw new InvalidOperationException("Invalid application type");
        }

        return applicationStore.GetPartnerTokenAsync(application, cancellationToken);
    }
}
