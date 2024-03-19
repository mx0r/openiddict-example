using Example.AuthServer.Api.Auth.Policies;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Example.AuthServer.Api.Auth.Extensions;

[PublicAPI]
public static class AuthorizationBuilderExtensions
{
    public static AuthorizationBuilder AddScopeRequirementPolicy(
        this AuthorizationBuilder builder, string scope, string issuer)
        => builder.AddPolicy(scope, policy => policy.Requirements.Add(
            new HasScopeRequirement(scope, issuer)));
}
