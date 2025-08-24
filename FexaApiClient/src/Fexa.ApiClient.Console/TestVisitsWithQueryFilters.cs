using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class TestVisitsWithQueryFilters
{
    public static async Task TestWithQueryStringFilters(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Testing Visits with Filters as Query Parameters ===\n");
        
        var tokenService = services.GetRequiredService<ITokenService>();
        var options = services.GetRequiredService<IOptions<FexaApiOptions>>().Value;
        
        try
        {
            // Get token first
            System.Console.WriteLine("Getting access token...");
            var token = await tokenService.GetAccessTokenAsync();
            System.Console.WriteLine("✅ Token acquired\n");
            
            // Test 1: Date range filter using "property" field name with start_date
            System.Console.WriteLine("Test 1: Date range filter with start_date (2025-08-14 to 2025-08-15)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var dateRangeFilter = new[]
            {
                new
                {
                    property = "visits.start_date",
                    @operator = "between",
                    value = new[] { "2025-08-14", "2025-08-15" }
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, dateRangeFilter, 0, 10, "Date range filter with start_date");
            
            // Test 2: Work order filter
            System.Console.WriteLine("\nTest 2: Work order filter (ID: 180901)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var workOrderFilter = new[]
            {
                new
                {
                    property = "workorders.id",
                    value = 180901
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, workOrderFilter, 0, 10, "Work order filter");
            
            // Test 3: Multiple work orders with IN operator
            System.Console.WriteLine("\nTest 3: Multiple work orders with IN operator");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var multiWorkOrderFilter = new[]
            {
                new
                {
                    property = "workorders.id",
                    @operator = "in",
                    value = new[] { 180901, 180902, 180903 }
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, multiWorkOrderFilter, 0, 10, "Multiple work orders");
            
            // Test 4: Try without the "visits." prefix
            System.Console.WriteLine("\nTest 4: Date filter without visits prefix (just 'start_date')");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var startDateFilter = new[]
            {
                new
                {
                    property = "start_date",
                    @operator = "between",
                    value = new[] { "2025-08-14", "2025-08-15" }
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, startDateFilter, 0, 10, "Start date filter without prefix");
            
            // Test 5: Work order without prefix
            System.Console.WriteLine("\nTest 5: Work order filter without prefix (just 'workorder_id')");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var workOrderNoPrefixFilter = new[]
            {
                new
                {
                    property = "workorder_id",
                    value = 180901
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, workOrderNoPrefixFilter, 0, 10, "Work order without prefix");
            
            // Test 6: Single day with datetime range (00:00:00 to 23:59:59)
            System.Console.WriteLine("\nTest 6: Single day filter with datetime range (2025-08-14)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var singleDayFilter = new[]
            {
                new
                {
                    property = "start_date",
                    @operator = "between",
                    value = new[] { "2025-08-14 00:00:00", "2025-08-14 23:59:59" }
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, singleDayFilter, 0, 10, "Single day with datetime");
            
            // Test 7: Single day with ISO 8601 format
            System.Console.WriteLine("\nTest 7: Single day filter with ISO 8601 format (2025-08-14)");
            System.Console.WriteLine("=" + new string('=', 50));
            
            var isoDateFilter = new[]
            {
                new
                {
                    property = "start_date",
                    @operator = "between",
                    value = new[] { "2025-08-14T00:00:00", "2025-08-14T23:59:59" }
                }
            };
            
            await SendGetWithQueryFilters(options.BaseUrl, token, isoDateFilter, 0, 10, "Single day ISO 8601");
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
    
    private static async Task SendGetWithQueryFilters(string baseUrl, string token, object filters, int start, int limit, string testName)
    {
        using var client = new HttpClient();
        
        try
        {
            // Serialize filters to JSON
            var filtersJson = JsonSerializer.Serialize(filters);
            System.Console.WriteLine($"Filter JSON: {filtersJson}");
            
            // URL encode the filters
            var encodedFilters = HttpUtility.UrlEncode(filtersJson);
            System.Console.WriteLine($"URL Encoded: {encodedFilters.Substring(0, Math.Min(100, encodedFilters.Length))}...\n");
            
            // Build the full URL with query parameters
            var url = $"{baseUrl}/api/ev1/visits?start={start}&limit={limit}&filters={encodedFilters}";
            System.Console.WriteLine($"Full URL: {url.Substring(0, Math.Min(150, url.Length))}...\n");
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
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
                            var workOrderId = visit.TryGetProperty("workorder_id", out var woId) && woId.ValueKind != JsonValueKind.Null
                                ? woId.GetInt32().ToString()
                                : "N/A";
                            var status = "unknown";
                            if (visit.TryGetProperty("object_state", out var objState) &&
                                objState.TryGetProperty("status", out var statusObj) &&
                                statusObj.TryGetProperty("name", out var statusName))
                            {
                                status = statusName.GetString() ?? "unknown";
                            }
                            
                            System.Console.WriteLine($"  Visit #{id}: Date: {startDate}, WO: {workOrderId}, Status: {status}");
                        }
                        
                        if (visitCount > 3)
                        {
                            System.Console.WriteLine($"  ... and {visitCount - 3} more");
                        }
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        System.Console.WriteLine($"❌ API Error: {error.GetString()}");
                        if (jsonDoc.RootElement.TryGetProperty("error_code", out var errorCode))
                        {
                            System.Console.WriteLine($"   Error Code: {errorCode.GetString()}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Response doesn't contain 'visits' or 'error' property");
                        System.Console.WriteLine($"Response keys: {string.Join(", ", GetJsonKeys(jsonDoc.RootElement))}");
                        if (responseContent.Length < 500)
                        {
                            System.Console.WriteLine($"Response: {responseContent}");
                        }
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
    
    private static string[] GetJsonKeys(JsonElement element)
    {
        var keys = new System.Collections.Generic.List<string>();
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                keys.Add(property.Name);
            }
        }
        return keys.ToArray();
    }
}