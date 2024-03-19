using System.ComponentModel.DataAnnotations.Schema;
using OpenIddict.EntityFrameworkCore.Models;

namespace Example.AuthServer.OpenIddict.Entities;

[Table("OpenIddictTokens")]
public class ExampleOpenIdToken
    : OpenIddictEntityFrameworkCoreToken<string, ExampleOpenIdApplication, ExampleOpenIdAuthorization>
{
    // empty
}