using Microsoft.EntityFrameworkCore;
using SpotifyStats.API.Data;
using SpotifyStats.API.Models;

namespace SpotifyStats.API.Services;

public class SpotifyTokenService : ISpotifyTokenService
{
    private readonly AppDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly ISpotifyAuthService _spotifyAuth;
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(60);

    public SpotifyTokenService(AppDbContext db, ISecretProtector protector, ISpotifyAuthService spotifyAuth)
    {
        _db = db;
        _protector = protector;
        _spotifyAuth = spotifyAuth;
    }

    public async Task<string> GetValidAccessTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var acct = await _db.SpotifyAccounts
            .SingleOrDefaultAsync(x => x.UserId == userId && !x.IsRevoked, ct);
        
        if (acct is null)
            throw new UnauthorizedAccessException("Spotify not connected.");

        // If we have a non-expired access token, return it
        if (acct.AccessTokenCiphertext is not null &&
            acct.AccessTokenExpiresAtUtc is not null &&
            acct.AccessTokenExpiresAtUtc.Value > DateTime.UtcNow.Add(ExpirySkew))
        {
            return _protector.Unprotect(acct.AccessTokenCiphertext);
        }

        // Otherwise refresh (with concurrency handling)
        return await RefreshAndReturnAsync(acct, ct);
    }

    public async Task StoreTokensAsync(Guid userId, string spotifyUserId, string accessToken, 
        string refreshToken, int expiresIn, string scope, CancellationToken ct = default)
    {
        var acct = await _db.SpotifyAccounts
            .SingleOrDefaultAsync(x => x.UserId == userId, ct);

        if (acct is null)
        {
            // Create new account
            acct = new SpotifyAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SpotifyUserId = spotifyUserId,
                RefreshTokenCiphertext = _protector.Protect(refreshToken),
                AccessTokenCiphertext = _protector.Protect(accessToken),
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn).Add(-ExpirySkew),
                Scope = scope,
                TokenType = "Bearer",
                IsRevoked = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.SpotifyAccounts.Add(acct);
        }
        else
        {
            // Update existing account
            acct.SpotifyUserId = spotifyUserId;
            acct.RefreshTokenCiphertext = _protector.Protect(refreshToken);
            acct.AccessTokenCiphertext = _protector.Protect(accessToken);
            acct.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn).Add(-ExpirySkew);
            acct.Scope = scope;
            acct.IsRevoked = false;
            acct.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var acct = await _db.SpotifyAccounts
            .SingleOrDefaultAsync(x => x.UserId == userId, ct);

        if (acct is not null)
        {
            acct.IsRevoked = true;
            acct.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<string> RefreshAndReturnAsync(SpotifyAccount acct, CancellationToken ct)
    {
        // Re-load inside a retry loop for concurrency exceptions
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                // Reload latest row
                acct = await _db.SpotifyAccounts.SingleAsync(x => x.Id == acct.Id, ct);

                // Another request may have refreshed already
                if (acct.AccessTokenCiphertext is not null &&
                    acct.AccessTokenExpiresAtUtc is not null &&
                    acct.AccessTokenExpiresAtUtc.Value > DateTime.UtcNow.Add(ExpirySkew))
                {
                    return _protector.Unprotect(acct.AccessTokenCiphertext);
                }

                var refreshToken = _protector.Unprotect(acct.RefreshTokenCiphertext);
                var refreshed = await _spotifyAuth.RefreshTokenAsync(refreshToken);

                // Note: refresh responses may NOT include refresh_token. Keep existing if null.
                if (!string.IsNullOrWhiteSpace(refreshed.refresh_token))
                    acct.RefreshTokenCiphertext = _protector.Protect(refreshed.refresh_token);

                acct.AccessTokenCiphertext = _protector.Protect(refreshed.access_token);
                acct.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(refreshed.expires_in).Add(-ExpirySkew);
                acct.TokenType = refreshed.token_type;
                acct.Scope = refreshed.scope ?? acct.Scope;
                acct.UpdatedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync(ct);

                return refreshed.access_token;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else updated the row. Retry once.
                if (attempt == 1) throw;
            }
        }

        throw new Exception("Token refresh retry failed unexpectedly.");
    }
}
