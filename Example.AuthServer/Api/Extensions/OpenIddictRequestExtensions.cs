using System.Security.Claims;
using Example.AuthServer.Api.Handlers.Exceptions;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.Domain.Entities;
using Example.AuthServer.OpenIddict.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;

namespace Example.AuthServer.Api.Extensions;

internal static class OpenIddictRequestExtensions
{
    private static string RequireUsername(this OpenIddictRequest request)
    {
        if (request.Username is null)
        {
            // when the username is NULL
            throw new InvalidOperationException("The username is NULL");
        }

        return request.Username;
    }

    public static async Task<Customer> RequireValidCustomerAsync(
        this OpenIddictRequest oidRequest, IServiceProvider sp, CancellationToken token = default)
    {
        var dsCustomer = sp.GetRequiredService<ICustomerDataSource>();

        // retrieve partner token from openid application
        var clientId = oidRequest.ClientId ?? throw new InvalidOperationException("Missing Client ID");
        var applicationManager = sp.GetRequiredService<OpenIddictApplicationManager<ExampleOpenIdApplication>>();
        var application = await applicationManager.FindByClientIdAsync(clientId, token);
        if (application is null)
        {
            // when the application is not available, throw an exception
            throw new InvalidOperationException("The application is available");
        }

        if (application.PartnerToken is not { } partnerToken)
        {
            // when the application is not a customer application, throw an exception;
            // password grant can be only provided for customer applications
            throw new InvalidOperationException("The application is not a customer application");
        }
        
        // get customer by email address
        var customer = await dsCustomer.GetByEmailAddressAsync(
            oidRequest.RequireUsername(), partnerToken,
            token);

        if (customer is null
            || !customer.IsPasswordValid(oidRequest.Password))
        {
            throw new AuthenticationForbiddenException(
                new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid username or password",
                });
        }

        return customer;
    }

    public static ClaimsIdentity ToClaimsIdentity(this Customer customer)
    {
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.AddClaim(OpenIddictConstants.Claims.Subject, customer.Urn);
        identity.AddClaim(OpenIddictConstants.Claims.Email, customer.EmailAddress);
        identity.AddClaim(OpenIddictConstants.Claims.Name, customer.FullName);

        return identity;
    }

    public static async Task<IEnumerable<string>> ToResources(
        this OpenIddictRequest oidRequest, 
        IServiceProvider sp, CancellationToken token = default)
    {
        var resources = new HashSet<string>();
        var requestScopes = oidRequest.GetScopes();
        var scopeManager = sp.GetRequiredService<IOpenIddictScopeManager>();
        await foreach (var scope in scopeManager.FindByNamesAsync(requestScopes, token))
        {
            var scopeResources = await scopeManager.GetResourcesAsync(scope, token);
            foreach (var scopeResource in scopeResources)
            {
                resources.Add(scopeResource);
            }
        }

        return resources;
    }
}
