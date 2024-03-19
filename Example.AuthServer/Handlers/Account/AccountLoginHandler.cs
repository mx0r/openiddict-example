using System.Security.Claims;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.OpenIddict.Managers;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Example.AuthServer.Handlers.Account;

[UsedImplicitly]
public class AccountLoginHandler(IServiceScopeFactory scopeFactory)
    : IRequestHandler<AccountLoginRequest, AccountLoginResponse>
{
    public async Task<AccountLoginResponse> Handle(
        AccountLoginRequest request, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var applicationManager = sp.GetRequiredService<ExampleOpenIdApplicationManager>();
        var clientApplication = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);

        var dsCustomer = sp.GetRequiredService<ICustomerDataSource>();

        // get customer by email address
        var customer = await dsCustomer.GetByEmailAddressAsync(
            request.Username, clientApplication!.PartnerToken!,
            cancellationToken);

        if (customer is null
            || !customer.IsPasswordValid(request.Password))
        {
            // invalid username or password; TODO: do a better error handling
            throw new Exception("Invalid username or password");
        }

        // login credentials are valid, create a cookie and continue with authorization
        var cookieClaims = new List<Claim>
        {
            // only add subject claim (customer URN) for now
            new(ClaimTypes.NameIdentifier, customer.Urn)
        };
        
        var claimsIdentity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        await request.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        
        return new AccountLoginResponse(request.RedirectUri);
    }
}

public record AccountLoginRequest(
    HttpContext HttpContext,
    string ClientId,
    string Username,
    string Password,
    string? RedirectUri = null)
    : IRequest<AccountLoginResponse>;

public record AccountLoginResponse(string? RedirectUri = null);
