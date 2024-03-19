using System.Text.Json.Serialization;

namespace Example.AuthServer.Api.V1.DataObjects;

public class ApplicationDto
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = null!;

    [JsonPropertyName("partnerToken")]
    public string? PartnerToken { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("redirectUrls")]
    public string? RedirectUris { get; set; }

    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }
}

public class PermissionDto
{
    [JsonPropertyName("type")]
    public PermissionTypes Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public enum PermissionTypes
{
    Endpoint,
    GrantType,
    ResponseType,
    Scope,
    Other
}
