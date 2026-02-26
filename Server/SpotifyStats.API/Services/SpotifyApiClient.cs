using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotifyStats.API.Services;

public class SpotifyApiClient
{
    private readonly HttpClient _http;
    private readonly ISpotifyTokenService _tokens;

    public SpotifyApiClient(HttpClient http, ISpotifyTokenService tokens)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.spotify.com/v1/");
        _tokens = tokens;
    }

    public async Task<JsonElement> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        return await CallSpotifyApiAsync(userId, HttpMethod.Get, "me", ct);
    }

    public async Task<JsonElement> GetTopTracksAsync(Guid userId, string timeRange = "medium_term", int limit = 20, CancellationToken ct = default)
    {
        return await CallSpotifyApiAsync(userId, HttpMethod.Get, $"me/top/tracks?time_range={timeRange}&limit={limit}", ct);
    }

    public async Task<JsonElement> GetTopArtistsAsync(Guid userId, string timeRange = "medium_term", int limit = 20, CancellationToken ct = default)
    {
        return await CallSpotifyApiAsync(userId, HttpMethod.Get, $"me/top/artists?time_range={timeRange}&limit={limit}", ct);
    }

    public async Task<JsonElement> GetRecentlyPlayedAsync(Guid userId, int limit = 20, CancellationToken ct = default)
    {
        return await CallSpotifyApiAsync(userId, HttpMethod.Get, $"me/player/recently-played?limit={limit}", ct);
    }

    private async Task<JsonElement> CallSpotifyApiAsync(Guid userId, HttpMethod method, string endpoint, CancellationToken ct, int retryCount = 0)
    {
        var accessToken = await _tokens.GetValidAccessTokenAsync(userId, ct);

        using var req = new HttpRequestMessage(method, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var res = await _http.SendAsync(req, ct);

        // Retry once on 401 (token might have been invalid despite expiry check)
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized && retryCount == 0)
        {
            // Force refresh by calling GetValidAccessTokenAsync again (it will refresh if needed)
            accessToken = await _tokens.GetValidAccessTokenAsync(userId, ct);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            res = await _http.SendAsync(req, ct);
        }

        res.EnsureSuccessStatusCode();

        var content = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<JsonElement>(content);
    }
}
