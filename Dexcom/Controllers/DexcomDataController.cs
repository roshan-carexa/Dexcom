using Microsoft.AspNetCore.Mvc;
using Dexcom.Services;

namespace Dexcom.Controllers;

[ApiController]
[Route("api/dexcom")]
public class DexcomDataController : ControllerBase
{
    private readonly IDexcomDataService _dataService;
    private readonly ILogger<DexcomDataController> _logger;

    public DexcomDataController(IDexcomDataService dataService, ILogger<DexcomDataController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet("egvs")]
    public async Task<IActionResult> GetEgvs(
        [FromQuery] string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var validationError = ValidateRequest(userId, startDate, endDate);
        if (validationError != null) return validationError;

        try
        {
            var data = await _dataService.GetEgvsAsync(userId, startDate, endDate);
            Console.WriteLine("Shayad data hai");
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = "Failed to fetch glucose data from Dexcom.", details = ex.Message });
        }
    }
    
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var validationError = ValidateRequest(userId, startDate, endDate);
        if (validationError != null) return validationError;

        try
        {
            var data = await _dataService.GetEventsAsync(userId, startDate, endDate);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch events for user: {UserId}", userId);
            return StatusCode(502, new { error = "Failed to fetch events from Dexcom.", details = ex.Message });
        }
    }
    
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var validationError = ValidateRequest(userId, startDate, endDate);
        if (validationError != null) return validationError;

        try
        {
            var data = await _dataService.GetAlertsAsync(userId, startDate, endDate);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch alerts for user: {UserId}", userId);
            return StatusCode(502, new { error = "Failed to fetch alerts from Dexcom.", details = ex.Message });
        }
    }
    
    [HttpGet("calibrations")]
    public async Task<IActionResult> GetCalibrations(
        [FromQuery] string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var validationError = ValidateRequest(userId, startDate, endDate);
        if (validationError != null) return validationError;

        try
        {
            var data = await _dataService.GetCalibrationsAsync(userId, startDate, endDate);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch calibrations for user: {UserId}", userId);
            return StatusCode(502, new { error = "Failed to fetch calibrations from Dexcom.", details = ex.Message });
        }
    }
    
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices(
        [FromQuery] string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var validationError = ValidateRequest(userId, startDate, endDate);
        if (validationError != null) return validationError;

        try
        {
            var data = await _dataService.GetDevicesAsync(userId, startDate, endDate);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch devices for user: {UserId}", userId);
            return StatusCode(502, new { error = "Failed to fetch devices from Dexcom.", details = ex.Message });
        }
    }
    
    [HttpGet("data-range")]
    public async Task<IActionResult> GetDataRange([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "userId is required." });
        }

        try
        {
            var data = await _dataService.GetDataRangeAsync(userId);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch data range for user: {UserId}", userId);
            return StatusCode(502, new { error = "Failed to fetch data range from Dexcom.", details = ex.Message });
        }
    }
    
    private IActionResult? ValidateRequest(string userId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "userId is required." });
        }

        if (startDate == default || endDate == default)
        {
            return BadRequest(new { error = "startDate and endDate are required. Use ISO 8601 format (e.g., 2025-01-01T00:00:00)." });
        }

        if (startDate >= endDate)
        {
            return BadRequest(new { error = "startDate must be before endDate." });
        }

        if ((endDate - startDate).TotalDays > 30)
        {
            return BadRequest(new { error = "Date range cannot exceed 30 days (Dexcom API limit)." });
        }

        return null;
    }
}
