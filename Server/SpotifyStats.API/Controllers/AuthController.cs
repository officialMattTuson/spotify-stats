using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyStats.API.Data;
using SpotifyStats.API.Extensions;
using SpotifyStats.API.Models;
using SpotifyStats.API.Scope;
using SpotifyStats.API.Services;

namespace SpotifyStats.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ISpotifyAuthService _authService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly SpotifyApiClient _spotifyApi;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IConfiguration config,
        ISpotifyAuthService authService,
        ISpotifyTokenService tokenService,
        SpotifyApiClient spotifyApi,
        IJwtService jwtService,
        AppDbContext db,
        ILogger<AuthController> logger)
    {
        _config = config;
        _authService = authService;
        _tokenService = tokenService;
        _spotifyApi = spotifyApi;
        _jwtService = jwtService;
        _db = db;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        try
        {
            var clientId = _config["Spotify:ClientId"] 
                ?? throw new InvalidOperationException("Spotify ClientId not configured");
            var redirectUri = _config["Spotify:RedirectUri"] 
                ?? throw new InvalidOperationException("Spotify RedirectUri not configured");
            var scope = SpotifyAPIScopes.All;
            var state = Guid.NewGuid().ToString("N");

            // Store state in cookie for validation
            Response.Cookies.Append("spotify_state", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromMinutes(10)
            });

            var authUrl = $"https://accounts.spotify.com/authorize?" +
                          $"client_id={clientId}" +
                          $"&response_type=code" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          $"&scope={Uri.EscapeDataString(scope)}" +
                          $"&state={state}";

            return Ok(new { authUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Spotify login URL");
            return StatusCode(500, new { error = "Failed to initiate login", details = ex.Message });
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string? error)
    {
        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";

        // Handle OAuth error
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Spotify OAuth error: {Error}", error);
            return Redirect($"{frontendUrl}/error?message={Uri.EscapeDataString(error)}");
        }

        // Validate state parameter
        var storedState = Request.Cookies["spotify_state"];
        if (string.IsNullOrEmpty(storedState) || storedState != state)
        {
            _logger.LogWarning("Invalid state parameter. Expected: {Expected}, Got: {Actual}", storedState, state);
            return Redirect($"{frontendUrl}/error?message={Uri.EscapeDataString("Invalid state parameter")}");
        }

        // Clear state cookie after validation
        Response.Cookies.Delete("spotify_state");

        if (string.IsNullOrEmpty(code))
        {
            return Redirect($"{frontendUrl}/error?message={Uri.EscapeDataString("Authorization code missing")}");
        }

        try
        {
            // Exchange code for tokens
            var tokenResponse = await _authService.ExchangeCodeAsync(code);

            // Get Spotify user profile
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);
            
            var meResponse = await httpClient.GetAsync("https://api.spotify.com/v1/me");
            
            if (!meResponse.IsSuccessStatusCode)
            {
                var errorContent = await meResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get Spotify user profile: {Error}", errorContent);
                return Redirect($"{frontendUrl}/error?message={Uri.EscapeDataString("Failed to retrieve Spotify profile")}");
            }

            var meContent = await meResponse.Content.ReadAsStringAsync();
            var meData = JsonSerializer.Deserialize<JsonElement>(meContent);
            var spotifyUserId = meData.GetProperty("id").GetString()!;

            // Get or create user
            var user = await _db.Users
                .Include(u => u.SpotifyAccount)
                .FirstOrDefaultAsync(u => 
                    u.SpotifyAccount != null && u.SpotifyAccount.SpotifyUserId == spotifyUserId);

            if (user == null)
            {
                // Create new user
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTime.UtcNow
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created new user {UserId} for Spotify user {SpotifyUserId}", 
                    user.Id, spotifyUserId);
            }

            // Store Spotify tokens
            await _tokenService.StoreTokensAsync(
                user.Id, 
                spotifyUserId, 
                tokenResponse.access_token, 
                tokenResponse.refresh_token!, 
                tokenResponse.expires_in, 
                tokenResponse.scope);

            // Generate JWT tokens
            var authTokens = _jwtService.GenerateTokens(user.Id);

            // Set HTTP-only cookies
            Response.Cookies.Append("auth_token", authTokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = authTokens.ExpiresAt
            });

            Response.Cookies.Append("refresh_token", authTokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            _logger.LogInformation("User {UserId} successfully authenticated with Spotify", user.Id);

            return Redirect($"{frontendUrl}/dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spotify authentication failed");
            return Redirect($"{frontendUrl}/error?message={Uri.EscapeDataString("Authentication failed. Please try again.")}");
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.GetUserId();
            await _tokenService.RevokeTokenAsync(userId);

            // Clear auth cookies
            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            return Ok(new { message = "Successfully logged out" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid authentication" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new { error = "Logout failed" });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.GetUserId();
            var userData = await _spotifyApi.GetMeAsync(userId);
            return Ok(userData);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Spotify not connected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user data");
            return StatusCode(500, new { error = "Failed to retrieve user data" });
        }
    }

    [Authorize]
    [HttpPost("refresh")]
    public IActionResult RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refresh_token"];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { error = "No refresh token provided" });
            }

            var principal = _jwtService.ValidateToken(refreshToken);
            
            if (principal == null)
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            var userId = principal.GetUserId();
            var newTokens = _jwtService.GenerateTokens(userId);

            // Update cookies with new tokens
            Response.Cookies.Append("auth_token", newTokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = newTokens.ExpiresAt
            });

            return Ok(new { message = "Token refreshed successfully", expiresAt = newTokens.ExpiresAt });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(new { error = "Token refresh failed" });
        }
    }
}
