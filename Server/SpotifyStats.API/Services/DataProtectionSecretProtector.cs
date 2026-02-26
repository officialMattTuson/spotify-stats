using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace SpotifyStats.API.Services;

public class DataProtectionSecretProtector : ISecretProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector("spotify.tokens.v1");

    public byte[] Protect(string plaintext)
        => Encoding.UTF8.GetBytes(_protector.Protect(plaintext));

    public string Unprotect(byte[] ciphertext)
        => _protector.Unprotect(Encoding.UTF8.GetString(ciphertext));
}
