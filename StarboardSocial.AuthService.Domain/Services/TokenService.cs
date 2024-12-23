using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Configuration;
using StarboardSocial.AuthService.Domain.Models;
using static FluentResults.Result;

namespace StarboardSocial.AuthService.Domain.Services;

public interface ITokenService
{
    Task<Result<Token>> ExchangeToken(string code, string redirectUri);
    Task<Result<Token>> RefreshToken(string refreshToken, string accessToken);
    Task<Result> RevokeToken(string userId);
}


public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly string[] _allowedRedirectUris;
    private readonly string _apiKey;
    private readonly string _tenantId;
    
    public TokenService(IConfiguration configuration)
    {
        _apiKey = configuration["Auth:ApiKey"]!;
        _tenantId = configuration["Auth:TenantId"]!;
        
        _httpClient = new HttpClient();
        string baseUrl = configuration["Auth:BaseUrl"]!;
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        string clientId = configuration["Auth:ClientId"]!;
        string clientSecret = configuration["Auth:ClientSecret"]!;
        string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        
        _allowedRedirectUris = configuration.GetSection("Auth:AllowedCallbackDomains")
            .AsEnumerable()
            .Select(x => x.Value)
            .Where(value => value != null)
            .ToArray()!;    
    }
    
    public async Task<Result<Token>> ExchangeToken(string code, string redirectUri)
    {
        if (!_allowedRedirectUris.Any(redirectUri.StartsWith))
        {
            return Fail("Redirect URI is not allowed");
        }
        
        List<KeyValuePair<string, string>> parameters =
        [
            new("grant_type", "authorization_code"),
            new("code", code),
            new("redirect_uri", redirectUri)
        ];

        HttpResponseMessage response =
            await _httpClient.PostAsync("/oauth2/token", new FormUrlEncodedContent(parameters));
        
        if (!response.IsSuccessStatusCode) return Fail("Failed to exchange token");
        Token token = (await response.Content.ReadFromJsonAsync<Token>())!;
        token.ExpiresAt = (int) DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn).ToUnixTimeSeconds();
        return Ok(token);
    }

    public async Task<Result<Token>> RefreshToken(string refreshToken, string accessToken)
    {
        List<KeyValuePair<string, string>> parameters =
        [
            new("grant_type", "refresh_token"),
            new("access_token", accessToken),
            new("refresh_token", refreshToken)
        ];
        
        HttpResponseMessage response =
            await _httpClient.PostAsync("/oauth2/token", new FormUrlEncodedContent(parameters));

        if (!response.IsSuccessStatusCode) return Fail("Failed to exchange refresh token");
        Token token = (await response.Content.ReadFromJsonAsync<Token>())!;
        token.ExpiresAt = (int) DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn).ToUnixTimeSeconds();
        return Ok(token);

    }

    public async Task<Result> RevokeToken(string userId)
    {
        _httpClient.DefaultRequestHeaders.Add("X-FusionAuth-TenantId", _tenantId);
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_apiKey);
        
        HttpResponseMessage response =
            await _httpClient.DeleteAsync($"/api/jwt/refresh?userId={userId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to revoke refreshToken of user with id: " + userId);
            Console.WriteLine(response);
            return Fail("Failed to revoke refreshToken");
        }
        return Ok();
    }
}