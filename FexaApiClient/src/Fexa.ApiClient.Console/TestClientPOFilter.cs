using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Fexa.ApiClient.Console;

public static class TestClientPOFilter
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var clientId = configuration["FexaApi:ClientId"];
        var clientSecret = configuration["FexaApi:ClientSecret"];
        var baseUrl = configuration["FexaApi:BaseUrl"] ?? "https://aafmapisandbox.fexa.io/";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            System.Console.WriteLine("API credentials not configured!");
            return;
        }

        // Get access token
        var token = await GetAccessTokenAsync(baseUrl, clientId, clientSecret);
        if (string.IsNullOrEmpty(token))
        {
            System.Console.WriteLine("Failed to get access token!");
            return;
        }

        System.Console.WriteLine($"Got access token: {token.Substring(0, 20)}...");
        
        // Test different filter approaches
        await TestFilterApproaches(baseUrl, token, "12345");
    }

    private static async Task<string?> GetAccessTokenAsync(string baseUrl, string clientId, string clientSecret)
    {
        using var client = new HttpClient();
        var tokenEndpoint = $"{baseUrl}oauth/token";
        
        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        });

        var response = await client.PostAsync(tokenEndpoint, requestBody);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            System.Console.WriteLine($"Token request failed: {error}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
        return tokenResponse.GetProperty("access_token").GetString();
    }

    private static async Task TestFilterApproaches(string baseUrl, string token, string poNumber)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var filterTests = new[]
        {
            // Test 1: Direct property path
            new { 
                Name = "Direct nested property", 
                Filter = new[] {
                    new Dictionary<string, object> {
                        ["property"] = "client_purchase_order_numbers.purchase_order_number",
                        ["operator"] = "equals",
                        ["value"] = poNumber
                    }
                }
            },
            // Test 2: Using workorders prefix
            new { 
                Name = "With workorders prefix", 
                Filter = new[] {
                    new Dictionary<string, object> {
                        ["property"] = "workorders.client_purchase_order_numbers.purchase_order_number",
                        ["operator"] = "equals",
                        ["value"] = poNumber
                    }
                }
            },
            // Test 3: Using contains operator
            new { 
                Name = "Contains operator", 
                Filter = new[] {
                    new Dictionary<string, object> {
                        ["property"] = "client_purchase_order_numbers.purchase_order_number",
                        ["operator"] = "contains",
                        ["value"] = poNumber
                    }
                }
            },
            // Test 4: Using like operator
            new { 
                Name = "Like operator", 
                Filter = new[] {
                    new Dictionary<string, object> {
                        ["property"] = "client_purchase_order_numbers.purchase_order_number",
                        ["operator"] = "like",
                        ["value"] = $"%{poNumber}%"
                    }
                }
            },
            // Test 5: Search in array (might work differently)
            new { 
                Name = "Search purchase_order_number field", 
                Filter = new[] {
                    new Dictionary<string, object> {
                        ["property"] = "purchase_order_number",
                        ["operator"] = "equals",
                        ["value"] = poNumber
                    }
                }
            }
        };

        foreach (var test in filterTests)
        {
            System.Console.WriteLine($"\n=== Testing: {test.Name} ===");
            
            var filterJson = JsonSerializer.Serialize(test.Filter);
            System.Console.WriteLine($"Filter JSON: {filterJson}");
            
            var encodedFilter = HttpUtility.UrlEncode(filterJson);
            var url = $"{baseUrl}api/ev1/workorders?filters={encodedFilter}";
            
            try
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                System.Console.WriteLine($"Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    if (result.TryGetProperty("workorders", out var workorders))
                    {
                        var count = workorders.GetArrayLength();
                        System.Console.WriteLine($"Found {count} work order(s)");
                        
                        if (count > 0)
                        {
                            System.Console.WriteLine("SUCCESS! This filter works!");
                            
                            // Show first work order's PO numbers
                            var firstWO = workorders[0];
                            if (firstWO.TryGetProperty("id", out var id))
                            {
                                System.Console.WriteLine($"Work Order ID: {id}");
                            }
                            if (firstWO.TryGetProperty("client_purchase_order_numbers", out var poNumbers))
                            {
                                System.Console.WriteLine("PO Numbers:");
                                foreach (var po in poNumbers.EnumerateArray())
                                {
                                    if (po.TryGetProperty("purchase_order_number", out var num))
                                    {
                                        System.Console.WriteLine($"  - {num}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine($"Error: {content}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception: {ex.Message}");
            }
        }
        
        // Also try without filter to see structure
        System.Console.WriteLine("\n=== Fetching a work order to see structure ===");
        var noFilterUrl = $"{baseUrl}api/ev1/workorders?limit=1";
        var sampleResponse = await client.GetAsync(noFilterUrl);
        if (sampleResponse.IsSuccessStatusCode)
        {
            var sampleContent = await sampleResponse.Content.ReadAsStringAsync();
            var sample = JsonSerializer.Deserialize<JsonElement>(sampleContent);
            if (sample.TryGetProperty("workorders", out var workorders) && workorders.GetArrayLength() > 0)
            {
                var wo = workorders[0];
                System.Console.WriteLine($"Sample work order structure:");
                if (wo.TryGetProperty("client_purchase_order_numbers", out var pos))
                {
                    System.Console.WriteLine($"client_purchase_order_numbers type: {pos.ValueKind}");
                    System.Console.WriteLine($"PO Numbers: {pos}");
                }
            }
        }
    }
}