namespace SpotifyStats.API.Services;

public interface ISpotifyTokenService
{
    Task<string> GetValidAccessTokenAsync(Guid userId, CancellationToken ct = default);
    Task StoreTokensAsync(Guid userId, string spotifyUserId, string accessToken, string refreshToken, int expiresIn, string scope, CancellationToken ct = default);
    Task RevokeTokenAsync(Guid userId, CancellationToken ct = default);
}
