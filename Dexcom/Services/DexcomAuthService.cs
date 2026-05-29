using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Dexcom.Data;
using Dexcom.Models;
using Dexcom.Settings;

namespace Dexcom.Services;

public class DexcomAuthService : IDexcomAuthService
{
    private readonly DexcomSettings _settings;
    private readonly DexcomDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DexcomAuthService> _logger;

    public DexcomAuthService(
        IOptions<DexcomSettings> settings,
        DexcomDbContext db,
        IHttpClientFactory httpClientFactory,
        ILogger<DexcomAuthService> logger)
    {
        _settings = settings.Value;
        _db = db;
        _httpClient = httpClientFactory.CreateClient("DexcomApi");
        _logger = logger;
    }
    public string GetLoginUrl()
    {
        var loginUrl = $"{_settings.BaseUrl}/v3/oauth2/login" +
                       $"?client_id={Uri.EscapeDataString(_settings.ClientId)}" +
                       $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
                       $"&response_type=code" +
                       $"&scope={Uri.EscapeDataString(_settings.Scopes)}";
        // + $"&state={Uri.EscapeDataString(state)}";

        
        // _logger.LogInformation("Generated Dexcom login URL for state: {State}", state);
        return loginUrl;
    }
    
    public async Task<DexcomUserToken> ExchangeCodeAsync(string userId, string code)
    {
        // _logger.LogInformation("Exchanging authorization code for user: {UserId}", userId);

        var tokenResponse = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _settings.RedirectUri
        });
        
        Console.WriteLine($"AccessTokenTime: {tokenResponse.ExpiresIn}");

        return await UpsertTokenAsync(userId, tokenResponse);
    }

    public async Task<string> GetValidAccessTokenAsync(string userId)
    {
        var token = await _db.DexcomUserTokens
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (token == null)
        {
            throw new InvalidOperationException($"No Dexcom tokens found for user: {userId}. User must connect their Dexcom account first.");
        }
        
        Console.WriteLine($"Expiry:{token.ExpiresAtUtc}");
        
        if (token.ExpiresAtUtc >= DateTime.UtcNow.AddMinutes(1))
        {
            return token.AccessToken;
        }

        _logger.LogInformation("Access token expired for user: {UserId}. Refreshing...", userId);
        return await RefreshAccessTokenAsync(token);
    }

    public async Task<bool> IsUserConnectedAsync(string userId)
    {
        return await _db.DexcomUserTokens.AnyAsync(t => t.UserId == userId);
    }

    public async Task DisconnectUserAsync(string userId)
    {
        var token = await _db.DexcomUserTokens
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (token != null)
        {
            _db.DexcomUserTokens.Remove(token);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Disconnected Dexcom account for user: {UserId}", userId);
        }
    }
    
    private async Task<string> RefreshAccessTokenAsync(DexcomUserToken token)
    {
        try
        {
            var tokenResponse = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = token.RefreshToken,
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret
            });
            var updated = await UpsertTokenAsync(token.UserId, tokenResponse);
            return updated.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to refresh token for user: {UserId}. User may need to re-authorize.", token.UserId);
            throw new InvalidOperationException(
                $"Failed to refresh Dexcom token for user: {token.UserId}", ex);
        }
    }
    
    private async Task<DexcomTokenResponse> RequestTokenAsync(Dictionary<string, string> formData)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v3/oauth2/token")
        {
            Content = new FormUrlEncodedContent(formData)
        };

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Dexcom token request failed. Status: {Status}, Body: {Body}",
                response.StatusCode, responseBody);
            throw new HttpRequestException(
                $"Dexcom token request failed with status {response.StatusCode}: {responseBody}");
        }

        var tokenResponse = JsonSerializer.Deserialize<DexcomTokenResponse>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Dexcom returned an invalid token response.");
        }
        return tokenResponse;
    }
    private async Task<DexcomUserToken> UpsertTokenAsync(string userId, DexcomTokenResponse tokenResponse)
    {
        var existing = await _db.DexcomUserTokens
            .FirstOrDefaultAsync(t => t.UserId == userId);
        
        if (existing != null)
        {
            existing.AccessToken = tokenResponse.AccessToken;
            existing.RefreshToken = tokenResponse.RefreshToken;
            existing.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            existing = new DexcomUserToken
            {
                UserId = userId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _db.DexcomUserTokens.Add(existing);
        }
    
        await _db.SaveChangesAsync();
        _logger.LogInformation("Stored/updated Dexcom tokens for user: {UserId}. Expires at: {ExpiresAt}",
            userId, existing.ExpiresAtUtc);
    
        return existing;
    }
   
}

internal class DexcomTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
