using System.ComponentModel.DataAnnotations.Schema;
using OpenIddict.EntityFrameworkCore.Models;

namespace Example.AuthServer.OpenIddict.Entities;

[Table("OpenIddictApplications")]
public class ExampleOpenIdApplication
    : OpenIddictEntityFrameworkCoreApplication<string, ExampleOpenIdAuthorization, ExampleOpenIdToken>
{
    [Column("PartnerToken")]
    public string? PartnerToken { get; set; }
}