using System.Text.Json;

namespace Dexcom.Services;

public interface IDexcomDataService
{

    Task<JsonElement> GetEgvsAsync(string userId, DateTime startDate, DateTime endDate);
    
    Task<JsonElement> GetEventsAsync(string userId, DateTime startDate, DateTime endDate);
    
    Task<JsonElement> GetAlertsAsync(string userId, DateTime startDate, DateTime endDate);
    
    Task<JsonElement> GetCalibrationsAsync(string userId, DateTime startDate, DateTime endDate);
    
    Task<JsonElement> GetDevicesAsync(string userId, DateTime startDate, DateTime endDate);
    Task<JsonElement> GetDataRangeAsync(string userId);
}
