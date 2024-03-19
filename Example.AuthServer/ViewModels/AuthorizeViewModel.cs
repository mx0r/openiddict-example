namespace Example.AuthServer.ViewModels;

public class AuthorizeViewModel
{
    public string ClientId { get; set; } = null!;
    public string? ReturnUrl { get; set; }
    public string ApplicationName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public IEnumerable<ScopeViewModel> Scopes { get; set; } = null!;
}

public class ScopeViewModel
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}
