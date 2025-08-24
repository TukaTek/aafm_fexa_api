using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Configuration;

namespace Fexa.ApiClient.Console;

public class TestFilterDebug
{
    public static async Task DebugFilterFormat(IServiceProvider services)
    {
        System.Console.WriteLine("\n=== Testing Filter Formats ===\n");
        
        var configuration = services.GetRequiredService<IConfiguration>();
        var tokenService = services.GetRequiredService<ITokenService>();
        
        // Get configuration
        var apiOptions = new FexaApiOptions();
        configuration.GetSection("FexaApi").Bind(apiOptions);
        
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
        
        // Test different filter formats
        var testCases = new[]
        {
            new
            {
                Name = "Visits - No filter",
                Url = "/api/ev1/visits?start=0&limit=5"
            },
            new
            {
                Name = "Client Invoices - No filter",
                Url = "/api/ev1/client_invoices?start=0&limit=5"
            },
            new
            {
                Name = "Client Invoices - Filter workorders.id (from PDF example)",
                Url = "/api/ev1/client_invoices?start=0&limit=5&filters=" + Uri.EscapeDataString("[{\"property\":\"workorders.id\",\"value\":116}]")
            },
            new
            {
                Name = "Client Invoices - Filter workorders.id = 180901",
                Url = "/api/ev1/client_invoices?start=0&limit=5&filters=" + Uri.EscapeDataString("[{\"property\":\"workorders.id\",\"value\":180901}]")
            },
            new
            {
                Name = "Workorders - No filter",
                Url = "/api/ev1/workorders?start=0&limit=5"
            },
            new
            {
                Name = "Workorders - Filter by id",
                Url = "/api/ev1/workorders?start=0&limit=5&filters=" + Uri.EscapeDataString("[{\"property\":\"workorders.id\",\"value\":180901}]")
            },
            new
            {
                Name = "Users - No filter",
                Url = "/api/ev1/users?start=0&limit=5"
            },
            new
            {
                Name = "Visits - Try filter param (not filters)",
                Url = "/api/ev1/visits?start=0&limit=5&filter=" + Uri.EscapeDataString("[{\"property\":\"workorder_id\",\"value\":180901}]")
            },
            new
            {
                Name = "Visits - Try query params instead of filters",
                Url = "/api/ev1/visits?start=0&limit=5&workorder_id=180901"
            }
        };
        
        foreach (var testCase in testCases)
        {
            System.Console.WriteLine($"\n{testCase.Name}:");
            System.Console.WriteLine($"URL: {testCase.Url}");
            
            try
            {
                var response = await httpClient.GetAsync(testCase.Url);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine($"✅ Status: {response.StatusCode}");
                    
                    // Parse and check if it's an error response
                    try
                    {
                        var json = JsonDocument.Parse(content);
                        if (json.RootElement.TryGetProperty("error", out var errorProp))
                        {
                            System.Console.WriteLine($"❌ API Error: {errorProp.GetString()}");
                            if (json.RootElement.TryGetProperty("error_code", out var errorCodeProp))
                            {
                                System.Console.WriteLine($"   Error Code: {errorCodeProp.GetString()}");
                            }
                        }
                        else if (json.RootElement.TryGetProperty("visits", out var visitsProp))
                        {
                            var visitsArray = visitsProp.EnumerateArray().ToList();
                            System.Console.WriteLine($"   Found {visitsArray.Count} visits");
                            if (visitsArray.Any())
                            {
                                System.Console.WriteLine($"   First visit ID: {visitsArray[0].GetProperty("id").GetInt32()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"   Parse error: {ex.Message}");
                        System.Console.WriteLine($"   Raw: {content.Substring(0, Math.Min(200, content.Length))}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ HTTP Status: {response.StatusCode}");
                    System.Console.WriteLine($"   Response: {content}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Exception: {ex.Message}");
            }
        }
        
        System.Console.WriteLine("\n\nPress any key to continue...");
        System.Console.ReadKey();
    }
}