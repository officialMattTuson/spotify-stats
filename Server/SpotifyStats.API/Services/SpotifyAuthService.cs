using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SpotifyStats.API.Models;

public interface ISpotifyAuthService
{
  Task<SpotifyTokenResponse> ExchangeCodeAsync(string code);
  Task<SpotifyTokenResponse> RefreshTokenAsync(string refreshToken);
}

public class SpotifyAuthService : ISpotifyAuthService
{
  private readonly HttpClient _httpClient;
  private readonly IConfiguration _config;

  public SpotifyAuthService(HttpClient httpClient, IConfiguration config)
  {
    _httpClient = httpClient;
    _config = config;
  }

  public async Task<SpotifyTokenResponse> ExchangeCodeAsync(string code)
  {
    var request = BuildTokenRequest(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _config["Spotify:RedirectUri"]! }
        });

    var response = await _httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
      throw new Exception($"Token exchange failed: {content}");

    return JsonSerializer.Deserialize<SpotifyTokenResponse>(content)!;
  }

  public async Task<SpotifyTokenResponse> RefreshTokenAsync(string refreshToken)
  {
    var request = BuildTokenRequest(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        });

    var response = await _httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
      throw new Exception($"Token refresh failed: {content}");

    return JsonSerializer.Deserialize<SpotifyTokenResponse>(content)!;
  }

  private HttpRequestMessage BuildTokenRequest(Dictionary<string, string> form)
  {
    var clientId = _config["Spotify:ClientId"];
    var clientSecret = _config["Spotify:ClientSecret"];

    var request = new HttpRequestMessage(
        HttpMethod.Post,
        "https://accounts.spotify.com/api/token");

    var basicAuth = Convert.ToBase64String(
        Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")
    );

    request.Headers.Authorization =
        new AuthenticationHeaderValue("Basic", basicAuth);

    request.Content = new FormUrlEncodedContent(form);

    return request;
  }
}
