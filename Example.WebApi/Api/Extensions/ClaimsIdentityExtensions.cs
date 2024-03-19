using System.Security.Claims;

namespace Example.WebApi.Api.Extensions;

public static class ClaimsIdentityExtensions
{
    public static Guid? GetSubjectId(this ClaimsIdentity identity)
        => identity.GetClaimValue(ClaimTypes.NameIdentifier) is { } subjectId
            ? Guid.Parse(subjectId)
            : null;
    
    public static string? GetGivenName(this ClaimsIdentity identity)
        => identity.GetClaimValue(ClaimTypes.GivenName);

    public static string? GetFamilyName(this ClaimsIdentity identity)
        => identity.GetClaimValue(ClaimTypes.Surname);

    public static string? GetClaimValue(this ClaimsIdentity identity, string claimType)
        => identity.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
}
