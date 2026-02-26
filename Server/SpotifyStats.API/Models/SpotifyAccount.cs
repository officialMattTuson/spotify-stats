namespace SpotifyStats.API.Models;

public class SpotifyAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    public string SpotifyUserId { get; set; } = default!;

    public byte[] RefreshTokenCiphertext { get; set; } = default!;
    public string? RefreshTokenKeyId { get; set; }

    public byte[]? AccessTokenCiphertext { get; set; }
    public DateTime? AccessTokenExpiresAtUtc { get; set; }

    public string Scope { get; set; } = "";
    public string TokenType { get; set; } = "Bearer";

    public bool IsRevoked { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Concurrency guard
    public byte[] RowVersion { get; set; } = default!;
}
