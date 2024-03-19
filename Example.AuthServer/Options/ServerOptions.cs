namespace Example.AuthServer.Options;

public class ServerOptions
{
    public string Issuer { get; set; }
    public string SigningCertificateFilePath { get; set; } = null!;
    public string EncryptionCertificateFilePath { get; set; } = null!;
}
