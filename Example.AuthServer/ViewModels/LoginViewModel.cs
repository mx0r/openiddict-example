using System.ComponentModel.DataAnnotations;

namespace Example.AuthServer.ViewModels;

public record LoginViewModel
{
    public string ClientId { get; set; } = null!;

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
