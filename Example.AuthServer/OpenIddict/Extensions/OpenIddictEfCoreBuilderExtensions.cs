using Example.AuthServer.OpenIddict.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.AuthServer.OpenIddict.Extensions;

internal static class OpenIddictEfCoreBuilderExtensions
{
    public static OpenIddictEntityFrameworkCoreBuilder UsePartnerAwareEntities(
        this OpenIddictEntityFrameworkCoreBuilder builder)
        => builder
            .ReplaceDefaultEntities<
                ExampleOpenIdApplication,
                ExampleOpenIdAuthorization,
                ExampleOpenIdScope,
                ExampleOpenIdToken,
                string>();

    public static DbContextOptionsBuilder UsePartnerAwareOpenIddict(
        this DbContextOptionsBuilder builder)
        => builder.UseOpenIddict<
            ExampleOpenIdApplication,
            ExampleOpenIdAuthorization,
            ExampleOpenIdScope,
            ExampleOpenIdToken,
            string>();
}