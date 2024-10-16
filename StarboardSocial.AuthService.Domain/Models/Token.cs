using System.Text.Json.Serialization;

namespace StarboardSocial.AuthService.Domain.Models;

public class Token
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }
    
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
    
    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }
    
    [JsonPropertyName("userId")]
    public required string UserId { get; init; }
}