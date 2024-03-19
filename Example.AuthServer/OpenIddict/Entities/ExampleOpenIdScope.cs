using System.ComponentModel.DataAnnotations.Schema;
using OpenIddict.EntityFrameworkCore.Models;

namespace Example.AuthServer.OpenIddict.Entities;

[Table("OpenIddictScopes")]
public class ExampleOpenIdScope
    : OpenIddictEntityFrameworkCoreScope<string>
{
    // empty
}