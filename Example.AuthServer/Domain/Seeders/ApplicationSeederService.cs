using Example.AuthServer.OpenIddict.Descriptors;
using Example.AuthServer.OpenIddict.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace Example.AuthServer.Domain.Seeders;

public class ApplicationSeederService(IServiceScopeFactory scopeFactory) 
    : IHostedService
{
    private readonly List<OpenIddictScopeDescriptor> _seededScopeDescriptors =
    [
        new OpenIddictScopeDescriptor
        {
            Name = "admin:auth",
            DisplayName = "AuthServer Administration",
            Description = "This scope allows the client to manage auth server",
            Resources = { "auth-server" }
        },
        new OpenIddictScopeDescriptor
        {
            Name = "read:weather",
            DisplayName = "Read weather",
            Description = "This scope allows the client to read weather data",
            Resources = { "example-api" }
        },
        new OpenIddictScopeDescriptor
        {
            Name = "write:weather",
            DisplayName = "Write weather",
            Description = "This scope allows the client to write weather data",
            Resources = { "example-api" }
        },
        new OpenIddictScopeDescriptor
        {
            Name = "read:users",
            DisplayName = "Read users",
            Description = "This scope allows the client to read user data",
            Resources = { "example-api" }
        },
        new OpenIddictScopeDescriptor
        {
            Name = "write:users",
            DisplayName = "Write users",
            Description = "This scope allows the client to write user data",
            Resources = { "example-api" }
        }
    ];
    
    private readonly List<OpenIddictApplicationDescriptor> _seededApplicationDescriptors =
    [
        // Internal auth server admin client token
        new ExampleOpenIdApplicationDescriptor
        {
            ClientId = "auth-admin",
            ClientSecret = "auth-password",
            DisplayName = "AuthServer Administrator",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Prefixes.Scope + "admin:auth",
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        },

        // Internal application client token
        new ExampleOpenIdApplicationDescriptor
        {
            ClientId = "machines-application-client",
            ClientSecret = "password",
            PartnerToken = "united-machines",
            DisplayName = "Example Application Client",
            RedirectUris =
            {
                new Uri("https://localhost:7777/oauth/userinfo")
            },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                OpenIddictConstants.Permissions.Prefixes.Scope + "read:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "read:users",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:users",
                
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        },

        // United Machines API client token
        new ExampleOpenIdApplicationDescriptor
        {
            ClientId = "machines-api-client",
            ClientSecret = "password",
            PartnerToken = "united-machines",
            DisplayName = "Example API Client",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                OpenIddictConstants.Permissions.Prefixes.Scope + "read:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "read:users",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:users",
                
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        },
        
        // Internal API client token
        new ExampleOpenIdApplicationDescriptor
        {
            ClientId = "internal-api-client",
            ClientSecret = "password",
            PartnerToken = "internal",
            DisplayName = "Example API Client",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                OpenIddictConstants.Permissions.Prefixes.Scope + "read:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:weather",
                OpenIddictConstants.Permissions.Prefixes.Scope + "read:users",
                OpenIddictConstants.Permissions.Prefixes.Scope + "write:users",
                
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        }
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = scopeFactory.CreateScope();
        var sp = serviceScope.ServiceProvider;

        var context = serviceScope.ServiceProvider.GetRequiredService<ServerDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var applicationManager = sp.GetRequiredService<OpenIddictApplicationManager<ExampleOpenIdApplication>>();
        foreach (var applicationDescriptor in _seededApplicationDescriptors)
        {
            var application = await applicationManager.FindByClientIdAsync(applicationDescriptor.ClientId!, cancellationToken);
            if (application is null)
            {
                // when the application is not found, create it
                await applicationManager.CreateAsync(applicationDescriptor, cancellationToken);
            }
        }

        var scopeManager = sp.GetRequiredService<IOpenIddictScopeManager>();
        foreach (var scopeDescriptor in _seededScopeDescriptors)
        {
            var scope = await scopeManager.FindByNameAsync(scopeDescriptor.Name!, cancellationToken);
            if (scope is null)
            {
                // when the scope is not found, create it
                await scopeManager.CreateAsync(scopeDescriptor, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
