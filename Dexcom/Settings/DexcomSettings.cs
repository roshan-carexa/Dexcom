namespace Dexcom.Settings;

public class DexcomSettings
{
    public const string SectionName = "Dexcom";
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "https://carexa.ai";
    public string BaseUrl { get; set; } = "https://sandbox-api.dexcom.com";
    public string Scopes { get; set; } = "offline_access";
}
