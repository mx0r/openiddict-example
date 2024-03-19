using System.ComponentModel.DataAnnotations.Schema;
using OpenIddict.EntityFrameworkCore.Models;

namespace Example.AuthServer.OpenIddict.Entities;

[Table("OpenIddictAuthorizations")]
public class ExampleOpenIdAuthorization
    : OpenIddictEntityFrameworkCoreAuthorization<string, ExampleOpenIdApplication, ExampleOpenIdToken>
{
    // empty
}
