using Microsoft.AspNetCore.Mvc;
using Dexcom.Services;

namespace Dexcom.Controllers;

[ApiController]
[Route("api/dexcom")]
public class DexcomAuthController : ControllerBase
{
    private readonly IDexcomAuthService _authService;
    private readonly ILogger<DexcomAuthController> _logger;

    public DexcomAuthController(IDexcomAuthService authService, ILogger<DexcomAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpGet("login")]
    public IActionResult Login()
    {
        var loginUrl = _authService.GetLoginUrl();
        return Redirect(loginUrl);
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        // if (!string.IsNullOrEmpty(error))
        // {
        //     _logger.LogWarning("Dexcom authorization denied. Error: {Error}", error);
        //     return BadRequest(new { error = "User denied Dexcom authorization.", details = error });
        // }
        //
        // if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        // {
        //     return BadRequest(new { error = "Missing authorization code or state parameter." });
        // }

        var userId = Guid.NewGuid().ToString(); 

        try
        {
            var token = await _authService.ExchangeCodeAsync(userId, code);

            return Ok(new
            {
                message = "Dexcom account connected successfully!",
                userid = userId,
                expiresAt = token.ExpiresAtUtc,
                connectedAt = token.CreatedAtUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange authorization code for user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to connect Dexcom account.", details = ex.Message });
        }
    }
    
    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetStatus(string userId)
    {
        var isConnected = await _authService.IsUserConnectedAsync(userId);
        return Ok(new { userId, isConnected });
    }

    [HttpDelete("revoke/{userId}")]
    public async Task<IActionResult> Revoke(string userId)
    {
        await _authService.DisconnectUserAsync(userId);
        _logger.LogInformation("Revoked Dexcom tokens for user: {UserId}", userId);
        return Ok(new { message = "Dexcom account disconnected.", userId });
    }
}
