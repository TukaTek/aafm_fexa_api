using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Console;

public class TestVisitDebug
{
    public static async Task DebugVisitApiCall(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<IFexaApiService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestVisitDebug>>();
        
        System.Console.WriteLine("\n=== Debugging Visit API Calls ===\n");
        
        // Test different possible endpoints
        var endpoints = new[]
        {
            "/api/ev1/visits",
            "/api/ev1/visit",
            "/api/v2/visits",
            "/api/v2/visit"
        };
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                System.Console.WriteLine($"Testing endpoint: {endpoint}");
                var testUrl = $"{endpoint}?start=0&limit=1";
                System.Console.WriteLine($"Full URL: {testUrl}");
                
                var response = await apiService.GetAsync<object>(testUrl);
                
                System.Console.WriteLine($"✅ SUCCESS: {endpoint} returned data");
                System.Console.WriteLine($"Response: {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}");
                break;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ FAILED: {endpoint} - {ex.Message}");
            }
        }
        
        // Also test with a simple filter
        System.Console.WriteLine("\n=== Testing with filters ===\n");
        
        var parameters = new QueryParameters
        {
            Start = 0,
            Limit = 5
        };
        
        var queryDict = parameters.ToDictionary();
        var queryString = string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        
        System.Console.WriteLine($"Query string: {queryString}");
        
        try
        {
            var testUrl = $"/api/ev1/visits?{queryString}";
            System.Console.WriteLine($"Testing: {testUrl}");
            
            var response = await apiService.GetAsync<object>(testUrl);
            System.Console.WriteLine("Response received:");
            System.Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
        }
    }
}