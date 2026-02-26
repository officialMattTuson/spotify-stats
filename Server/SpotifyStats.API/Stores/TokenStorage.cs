using SpotifyStats.API.Models;

namespace SpotifyStats.API.Store;
public interface ISpotifyTokenStore
{
    void Save(string userId, SpotifyTokenResponse token);
    SpotifyTokenResponse? Get(string userId);
    void Clear(string userId);
}

public class InMemorySpotifyTokenStore : ISpotifyTokenStore
{
    private readonly Dictionary<string, SpotifyTokenResponse> _tokens = new();

    public void Save(string userId, SpotifyTokenResponse token)
    {
        _tokens[userId] = token;
    }

    public SpotifyTokenResponse? Get(string userId)
    {
        _tokens.TryGetValue(userId, out var token);
        return token;
    }

    public void Clear(string userId)
    {
        _tokens.Remove(userId);
    }
}
