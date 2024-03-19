using Microsoft.AspNetCore.Authorization;

namespace Example.WebApi.Auth.Policies;

public class HasScopeRequirement(string scope, string issuer)
    : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
    public string Issuer { get; } = issuer;
}

public class HasScopeRequirementHandler
    : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasScopeRequirement requirement)
    {
        var scopeClaim = context.User.FindFirst(
            c => c.Type == "scope"
                 && c.Issuer == requirement.Issuer);

        if (scopeClaim is null)
        {
            // when no scope claim found, skip...
            return Task.CompletedTask;
        }

        // get the scopes from the claim
        var scopes = scopeClaim.Value.Split(' ');

        // when there is a required scope in the array -> succeed
        if (scopes.Any(s => s == requirement.Scope))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
