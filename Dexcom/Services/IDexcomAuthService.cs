using Dexcom.Models;

namespace Dexcom.Services;

public interface IDexcomAuthService
{

    string GetLoginUrl();
    Task<DexcomUserToken> ExchangeCodeAsync(string userId, string code);
    Task<string> GetValidAccessTokenAsync(string userId);
    Task<bool> IsUserConnectedAsync(string userId);
    Task DisconnectUserAsync(string userId);
}
