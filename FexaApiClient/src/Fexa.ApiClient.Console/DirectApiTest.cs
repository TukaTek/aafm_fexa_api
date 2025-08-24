using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Configuration;

namespace Fexa.ApiClient.Console;

public class DirectApiTest
{
    public static async Task TestDirectApiCall(IServiceProvider services)
    {
        System.Console.WriteLine("\n=== Direct API Test ===\n");
        
        var configuration = services.GetRequiredService<IConfiguration>();
        var tokenService = services.GetRequiredService<ITokenService>();
        
        // Get configuration
        var apiOptions = new FexaApiOptions();
        configuration.GetSection("FexaApi").Bind(apiOptions);
        
        System.Console.WriteLine($"Base URL: {apiOptions.BaseUrl}");
        
        // Get token
        var tokenResponse = await tokenService.RefreshTokenAsync();
        System.Console.WriteLine($"Got token: {tokenResponse.AccessToken.Substring(0, 20)}...");
        
        // Create HttpClient
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(apiOptions.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        
        // Test various endpoints
        var testEndpoints = new[]
        {
            "/api/ev1/visits?start=0&limit=1",
            "/api/v2/visits?start=0&limit=1",
            "/api/ev1/client_invoices?start=0&limit=1",
            "/api/ev1/users?start=0&limit=1",
            "/api/ev1/workorders?start=0&limit=1",
            "/api/ev1/vendors?start=0&limit=1",
            "/api/ev1/technicians?start=0&limit=1",
            "/api/ev1/locations?start=0&limit=1"
        };
        
        foreach (var endpoint in testEndpoints)
        {
            System.Console.WriteLine($"\nTesting: {endpoint}");
            
            try
            {
                var response = await httpClient.GetAsync(endpoint);
                System.Console.WriteLine($"Status: {response.StatusCode}");
                
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine("✅ SUCCESS");
                    
                    // Try to parse and display formatted JSON
                    try
                    {
                        var json = JsonDocument.Parse(content);
                        System.Console.WriteLine($"Response preview: {JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions { WriteIndented = true, MaxDepth = 2 }).Substring(0, Math.Min(500, content.Length))}...");
                    }
                    catch
                    {
                        System.Console.WriteLine($"Response: {content.Substring(0, Math.Min(200, content.Length))}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ FAILED");
                    System.Console.WriteLine($"Response: {content}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
            }
        }
    }
}