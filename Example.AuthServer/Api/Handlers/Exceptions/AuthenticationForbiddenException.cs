using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;

namespace Example.AuthServer.Api.Handlers.Exceptions;

[PublicAPI]
public class AuthenticationForbiddenException
    : Exception
{
    public AuthenticationProperties? Properties { get; }

    public AuthenticationForbiddenException(Dictionary<string, string>? properties = null)
        : this(properties, null)
    {
        // empty
    }

    public AuthenticationForbiddenException(
        Dictionary<string, string>? properties, Exception? innerException)
        : this("Authentication forbidden", properties, innerException)
    {
        // empty
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public AuthenticationForbiddenException(
        string? message, Dictionary<string, string>? properties, Exception? innerException)
        : base(message, innerException)
    {
        Properties = properties is not null ? new AuthenticationProperties(properties!) : null;
    }
}
