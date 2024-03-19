using Example.WebApi.Auth.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Example.WebApi.Auth.Extensions;

public static class AuthorizationBuilderExtensions
{
    public static AuthorizationBuilder AddScopeRequirementPolicy(
        this AuthorizationBuilder builder, string scope, string issuer)
        => builder.AddPolicy(scope, policy => policy.Requirements.Add(
            new HasScopeRequirement(scope, issuer)));
}
