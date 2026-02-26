using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyStats.API.Extensions;
using SpotifyStats.API.Services;

namespace SpotifyStats.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SpotifyController : ControllerBase
{
    private readonly SpotifyApiClient _spotifyApi;
    private readonly ILogger<SpotifyController> _logger;

    public SpotifyController(SpotifyApiClient spotifyApi, ILogger<SpotifyController> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = User.GetUserId();
            var userData = await _spotifyApi.GetMeAsync(userId);
            return Ok(userData);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Spotify not connected for user");
            return Unauthorized(new { error = "Spotify account not connected", message = "Please connect your Spotify account first" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Spotify user data");
            return StatusCode(500, new { error = "Failed to retrieve user data", message = ex.Message });
        }
    }

    [HttpGet("top/tracks")]
    public async Task<IActionResult> GetTopTracks(
        [FromQuery] string timeRange = "medium_term",
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = User.GetUserId();
            var tracks = await _spotifyApi.GetTopTracksAsync(userId, timeRange, limit);
            return Ok(tracks);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Spotify not connected for user");
            return Unauthorized(new { error = "Spotify account not connected", message = "Please connect your Spotify account first" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top tracks");
            return StatusCode(500, new { error = "Failed to retrieve top tracks", message = ex.Message });
        }
    }

    [HttpGet("top/artists")]
    public async Task<IActionResult> GetTopArtists(
        [FromQuery] string timeRange = "medium_term",
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = User.GetUserId();
            var artists = await _spotifyApi.GetTopArtistsAsync(userId, timeRange, limit);
            return Ok(artists);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Spotify not connected for user");
            return Unauthorized(new { error = "Spotify account not connected", message = "Please connect your Spotify account first" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top artists");
            return StatusCode(500, new { error = "Failed to retrieve top artists", message = ex.Message });
        }
    }

    [HttpGet("recently-played")]
    public async Task<IActionResult> GetRecentlyPlayed([FromQuery] int limit = 20)
    {
        try
        {
            var userId = User.GetUserId();
            var tracks = await _spotifyApi.GetRecentlyPlayedAsync(userId, limit);
            return Ok(tracks);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Spotify not connected for user");
            return Unauthorized(new { error = "Spotify account not connected", message = "Please connect your Spotify account first" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recently played tracks");
            return StatusCode(500, new { error = "Failed to retrieve recently played tracks", message = ex.Message });
        }
    }
}
