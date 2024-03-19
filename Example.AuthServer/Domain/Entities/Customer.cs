using System.ComponentModel.DataAnnotations.Schema;

namespace Example.AuthServer.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Partner Partner { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PasswordPlain { get; set; } = null!;

    [NotMapped]
    public string Urn => $"customer:{Id}";

    public bool IsPasswordValid(string? checkedPassword)
        => checkedPassword is not null
           && checkedPassword == PasswordPlain;
}
