namespace SpotifyStats.API.Services;

public interface ISecretProtector
{
    byte[] Protect(string plaintext);
    string Unprotect(byte[] ciphertext);
}
