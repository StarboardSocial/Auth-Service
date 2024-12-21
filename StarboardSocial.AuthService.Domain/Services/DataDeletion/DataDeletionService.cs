

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace StarboardSocial.UserService.Domain.Services;

public interface IDataDeletionService
{
    Task DeleteUserData(string userId);
}

public class DataDeletionService : IDataDeletionService
{

    private readonly HttpClient _httpClient;
    
    public DataDeletionService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        string baseUrl = configuration["Auth:BaseUrl"]!;
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        string basicAuth = configuration["Auth:ApiKey"]!;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(basicAuth);
        _httpClient.DefaultRequestHeaders.Add("X-FusionAuth-TenantId", configuration["Auth:TenantId"]!);
    }
    
    public async Task DeleteUserData(string userId)
    {
        HttpResponseMessage response =
            await _httpClient.DeleteAsync($"/api/user/{userId}?hardDelete=true");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to delete user data of user with id: " + userId);
            throw new Exception("Failed to delete user data");
        }
    }
    
}