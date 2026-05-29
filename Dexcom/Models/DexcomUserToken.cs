using System.ComponentModel.DataAnnotations;

namespace Dexcom.Models;

public class DexcomUserToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
