using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class TestVisitsWithBody
{
    public static async Task TestGetWithBody(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Testing GET Visits with Filters in Body ===\n");
        
        var tokenService = services.GetRequiredService<ITokenService>();
        var options = services.GetRequiredService<IOptions<FexaApiOptions>>().Value;
        
        try
        {
            // Get token first
            System.Console.WriteLine("Getting access token...");
            var token = await tokenService.GetAccessTokenAsync();
            System.Console.WriteLine("✅ Token acquired\n");
            
            // Test 1: Date range filter
            System.Console.WriteLine("Test 1: Date range filter (2025-08-14 to 2025-08-15)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var dateRangeFilter = new
            {
                filters = new[]
                {
                    new
                    {
                        key = "visits.scheduled_date",
                        @operator = "between",
                        value = new[] { "2025-08-14", "2025-08-15" }
                    }
                },
                start = 0,
                limit = 10
            };
            
            await SendGetWithBody(options.BaseUrl, token, dateRangeFilter, "Date range filter");
            
            // Test 2: Work order filter
            System.Console.WriteLine("\nTest 2: Work order filter (ID: 180901)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var workOrderFilter = new
            {
                filters = new[]
                {
                    new
                    {
                        key = "workorders.id",
                        @operator = "equals",
                        value = 180901
                    }
                },
                start = 0,
                limit = 10
            };
            
            await SendGetWithBody(options.BaseUrl, token, workOrderFilter, "Work order filter");
            
            // Test 3: Status filter
            System.Console.WriteLine("\nTest 3: Status filter (completed)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var statusFilter = new
            {
                filters = new[]
                {
                    new
                    {
                        key = "visits.status",
                        @operator = "equals", 
                        value = "completed"
                    }
                },
                start = 0,
                limit = 10
            };
            
            await SendGetWithBody(options.BaseUrl, token, statusFilter, "Status filter");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
    }
    
    private static async Task SendGetWithBody(string baseUrl, string token, object body, string testName)
    {
        using var client = new HttpClient();
        
        try
        {
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            System.Console.WriteLine($"Sending {testName}:");
            System.Console.WriteLine($"Body: {json}");
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{baseUrl}/api/ev1/visits"),
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                Headers =
                {
                    { "Authorization", $"Bearer {token}" },
                    { "Accept", "application/json" }
                }
            };
            
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            System.Console.WriteLine($"Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // Parse and show visit count
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    if (jsonDoc.RootElement.TryGetProperty("visits", out var visits))
                    {
                        var visitCount = visits.GetArrayLength();
                        System.Console.WriteLine($"✅ Success: Returned {visitCount} visits");
                        
                        // Show first few visits
                        int shown = 0;
                        foreach (var visit in visits.EnumerateArray())
                        {
                            if (shown++ >= 3) break;
                            
                            var id = visit.GetProperty("id").GetInt32();
                            var startDate = visit.TryGetProperty("start_date", out var sd) && sd.ValueKind != JsonValueKind.Null
                                ? sd.GetDateTime().ToString("yyyy-MM-dd")
                                : "N/A";
                            var status = "unknown";
                            if (visit.TryGetProperty("object_state", out var objState) &&
                                objState.TryGetProperty("status", out var statusObj) &&
                                statusObj.TryGetProperty("name", out var statusName))
                            {
                                status = statusName.GetString() ?? "unknown";
                            }
                            
                            System.Console.WriteLine($"  Visit #{id}: Date: {startDate}, Status: {status}");
                        }
                        
                        if (visitCount > 3)
                        {
                            System.Console.WriteLine($"  ... and {visitCount - 3} more");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Response doesn't contain 'visits' property");
                        System.Console.WriteLine($"Response structure: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Could not parse response: {ex.Message}");
                    if (responseContent.Length < 500)
                    {
                        System.Console.WriteLine($"Response: {responseContent}");
                    }
                }
            }
            else
            {
                System.Console.WriteLine($"❌ Request failed");
                if (responseContent.Length < 500)
                {
                    System.Console.WriteLine($"Response: {responseContent}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Exception: {ex.Message}");
        }
    }
}