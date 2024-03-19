using System.ComponentModel.DataAnnotations.Schema;

namespace Example.AuthServer.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string Name { get; set; }
    
    [NotMapped]
    public string Urn => $"partner:{Id}";
}
