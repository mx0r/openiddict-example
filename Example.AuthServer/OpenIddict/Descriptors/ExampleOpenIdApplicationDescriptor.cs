using OpenIddict.Abstractions;

namespace Example.AuthServer.OpenIddict.Descriptors;

public class ExampleOpenIdApplicationDescriptor
    : OpenIddictApplicationDescriptor
{
    public string? PartnerToken { get; set; }
}
