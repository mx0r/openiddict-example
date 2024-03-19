namespace Example.WebApi.Auth.Options;

public class AuthOptions
{
    public string Authority { get; set; }
    public string Audience { get; set; }
    public string SigningCertificateFilePath { get; set; }
}
