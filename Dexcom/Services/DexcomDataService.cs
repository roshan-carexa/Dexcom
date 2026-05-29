using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Dexcom.Settings;

namespace Dexcom.Services;

public class DexcomDataService : IDexcomDataService
{
    private readonly IDexcomAuthService _authService;
    private readonly DexcomSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DexcomDataService> _logger;

    public DexcomDataService(
        IDexcomAuthService authService,
        IOptions<DexcomSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<DexcomDataService> logger)
    {
        _authService = authService;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient("DexcomApi");
        _logger = logger;
    }
    
    public async Task<JsonElement> GetEgvsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/egvs", startDate, endDate);
    }

    public async Task<JsonElement> GetEventsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/events", startDate, endDate);
    }

    public async Task<JsonElement> GetAlertsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/alerts", startDate, endDate);
    }

    public async Task<JsonElement> GetCalibrationsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/calibrations", startDate, endDate);
    }
    
    public async Task<JsonElement> GetDevicesAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/devices", startDate, endDate);
    }

    public async Task<JsonElement> GetDataRangeAsync(string userId)
    {
        return await GetDexcomDataAsync(userId, "/v3/users/self/dataRange", null, null);
    }
    
    private async Task<JsonElement> GetDexcomDataAsync(
        string userId,
        string endpoint,
        DateTime? startDate,
        DateTime? endDate)
    {
        var accessToken = await _authService.GetValidAccessTokenAsync(userId);
        Console.WriteLine($"AccessToken: {accessToken}");
        var response = await MakeApiRequestAsync(endpoint, accessToken, startDate, endDate);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Received 401 from Dexcom for user: {UserId}. Attempting token refresh...", userId);
            accessToken = await _authService.GetValidAccessTokenAsync(userId);
            response = await MakeApiRequestAsync(endpoint, accessToken, startDate, endDate);
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Dexcom API call failed. Endpoint: {Endpoint}, Status: {Status}, Body: {Body}",
                endpoint, response.StatusCode, responseBody);
            throw new HttpRequestException(
                $"Dexcom API request to {endpoint} failed with status {response.StatusCode}: {responseBody}");
        }

        var jsonDoc = JsonDocument.Parse(responseBody);
        return jsonDoc.RootElement.Clone();
    }
    
    private async Task<HttpResponseMessage> MakeApiRequestAsync(
        string endpoint,
        string accessToken,
        DateTime? startDate,
        DateTime? endDate)
    {
        var url = $"{_settings.BaseUrl}{endpoint}";

        if (startDate.HasValue && endDate.HasValue)
        {
            var start = startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            var end = endDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            url += $"?startDate={Uri.EscapeDataString(start)}&endDate={Uri.EscapeDataString(end)}";
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        _logger.LogDebug("Making Dexcom API request: GET {Url}", url);
        var response = await _httpClient.SendAsync(request);
        Console.WriteLine(response.StatusCode);
        return response;
    }
}
