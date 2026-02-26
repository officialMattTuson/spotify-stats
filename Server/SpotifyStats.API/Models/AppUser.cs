namespace SpotifyStats.API.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public SpotifyAccount? SpotifyAccount { get; set; }
}
