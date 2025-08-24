using System.Text.Json;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Console;

public static class TestWorkOrderDebug
{
    public static async Task DebugWorkOrderApiCall(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Work Order API Debug Testing ===\n");
        
        using var scope = services.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<IFexaApiService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IFexaApiService>>();
        
        System.Console.WriteLine("Select test type:");
        System.Console.WriteLine("1. Test basic work order endpoint");
        System.Console.WriteLine("2. Test work order with direct query parameters");
        System.Console.WriteLine("3. Test work order with filters array (GET with query)");
        System.Console.WriteLine("4. Test work order filter formats");
        System.Console.WriteLine("5. Test all filter variations");
        System.Console.WriteLine("6. Test POST with filters in body");
        System.Console.WriteLine("7. Test GET with filters in body");
        System.Console.WriteLine("8. Quick Status Filter Test (All Variations)");
        System.Console.Write("\nEnter choice (1-8): ");
        
        var choice = System.Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                await TestBasicEndpoint(apiService);
                break;
            case "2":
                await TestDirectQueryParams(apiService);
                break;
            case "3":
                await TestFiltersArray(apiService);
                break;
            case "4":
                await TestFilterFormats(apiService);
                break;
            case "5":
                await TestAllVariations(apiService);
                break;
            case "6":
                await TestPostWithBody(apiService);
                break;
            case "7":
                await TestGetWithBody(apiService);
                break;
            case "8":
                await QuickStatusFilterTest(apiService);
                break;
            default:
                System.Console.WriteLine("Invalid choice");
                break;
        }
    }
    
    private static async Task TestBasicEndpoint(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing Basic Endpoint ---");
        
        var endpoint = "/api/ev1/workorders?start=0&limit=5";
        System.Console.WriteLine($"Endpoint: {endpoint}");
        
        try
        {
            var response = await apiService.GetAsync<object>(endpoint);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            
            // Check for pagination info
            System.Console.WriteLine($"Full Response Structure (first 2000 chars):\n{json.Substring(0, Math.Min(2000, json.Length))}...");
            
            // Try to parse and show key structure
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                System.Console.WriteLine("\n=== Response Structure ===");
                foreach (var key in parsed.Keys)
                {
                    System.Console.WriteLine($"- Key: '{key}'");
                    if (key == "workorders" && parsed[key].ValueKind == JsonValueKind.Array)
                    {
                        System.Console.WriteLine($"  - Array length: {parsed[key].GetArrayLength()}");
                    }
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    private static async Task TestDirectQueryParams(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing Direct Query Parameters ---");
        
        System.Console.Write("Enter field name (e.g., status, vendor_id, client_id, assigned_to, placed_for): ");
        var field = System.Console.ReadLine();
        System.Console.Write("Enter value: ");
        var value = System.Console.ReadLine();
        
        // Try different variations
        var endpoints = new List<string>
        {
            $"/api/ev1/workorders?{field}={value}&start=0&limit=5",
            $"/api/ev1/workorders?workorders.{field}={value}&start=0&limit=5",
        };
        
        // Add special cases for specific fields
        if (field?.ToLower().Contains("status") == true)
        {
            endpoints.Add($"/api/ev1/workorders?status={value}&start=0&limit=5");
            endpoints.Add($"/api/ev1/workorders?object_state.status.name={value}&start=0&limit=5");
            endpoints.Add($"/api/ev1/workorders?workorders.object_state.status.name={value}&start=0&limit=5");
        }
        else if (field?.ToLower() == "vendor" || field?.ToLower() == "vendor_id")
        {
            endpoints.Add($"/api/ev1/workorders?assigned_to={value}&start=0&limit=5");
            endpoints.Add($"/api/ev1/workorders?vendor_id={value}&start=0&limit=5");
        }
        else if (field?.ToLower() == "client" || field?.ToLower() == "client_id")
        {
            endpoints.Add($"/api/ev1/workorders?placed_for={value}&start=0&limit=5");
            endpoints.Add($"/api/ev1/workorders?client_id={value}&start=0&limit=5");
        }
        
        foreach (var endpoint in endpoints)
        {
            System.Console.WriteLine($"\nTrying: {endpoint}");
            
            try
            {
                var response = await apiService.GetAsync<object>(endpoint);
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
                System.Console.WriteLine($"Success! Response length: {json.Length} chars");
                
                // Check if we got actual results
                if (json.Contains("\"workorders\":["))
                {
                    System.Console.WriteLine("✓ Got work orders array");
                    // Try to parse and count results
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                        if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                        {
                            var count = parsed["workorders"].GetArrayLength();
                            System.Console.WriteLine($"✓ Found {count} work order(s)");
                        }
                    }
                    catch { }
                }
                else if (json.Contains("\"workorders\":{"))
                {
                    System.Console.WriteLine("✓ Got single work order");
                }
                else if (json.Contains("\"error\""))
                {
                    System.Console.WriteLine("✗ Got error response");
                    // Show the error
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                        if (parsed?.ContainsKey("error") == true)
                        {
                            System.Console.WriteLine($"   Error: {parsed["error"]}");
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }
    
    private static async Task TestFiltersArray(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing Filters Array ---");
        
        System.Console.Write("Enter field to filter (e.g., status, vendor_id): ");
        var field = System.Console.ReadLine();
        System.Console.Write("Enter value: ");
        var value = System.Console.ReadLine();
        
        // Test different filter formats
        var filterFormats = new[]
        {
            // Format 1: Standard Fexa format
            new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = field,
                    ["value"] = value
                }
            },
            // Format 2: With operator
            new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = field,
                    ["operator"] = "equals",
                    ["value"] = value
                }
            },
            // Format 3: With workorders prefix
            new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = $"workorders.{field}",
                    ["value"] = value
                }
            },
            // Format 4: For nested status
            new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = "object_state.status.name",
                    ["value"] = value
                }
            }
        };
        
        int formatNumber = 1;
        foreach (var filter in filterFormats)
        {
            System.Console.WriteLine($"\n--- Format {formatNumber++} ---");
            
            var filterJson = JsonSerializer.Serialize(filter);
            System.Console.WriteLine($"Filter JSON: {filterJson}");
            
            var encodedFilter = HttpUtility.UrlEncode(filterJson);
            var endpoint = $"/api/ev1/workorders?filters={encodedFilter}&start=0&limit=5";
            
            System.Console.WriteLine($"Encoded URL: ...?filters={encodedFilter.Substring(0, Math.Min(50, encodedFilter.Length))}...");
            
            try
            {
                var response = await apiService.GetAsync<object>(endpoint);
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
                System.Console.WriteLine($"✓ Success! Response length: {json.Length} chars");
                
                // Check for actual results
                if (json.Contains("\"workorders\":[") || json.Contains("\"workorders\":{"))
                {
                    System.Console.WriteLine("✓ Got work order data");
                }
                else if (json.Contains("\"error\""))
                {
                    System.Console.WriteLine("✗ Got error response");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }
    
    private static async Task TestFilterFormats(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing Different Filter Formats ---");
        
        // Test status filtering with different approaches
        var testCases = new[]
        {
            ("Direct status param", "/api/ev1/workorders?status=New&start=0&limit=5"),
            ("Direct object_state.status.name", "/api/ev1/workorders?object_state.status.name=New&start=0&limit=5"),
            ("Filter with status", BuildFilterEndpoint("status", "New")),
            ("Filter with object_state.status.name", BuildFilterEndpoint("object_state.status.name", "New")),
            ("Filter with workorders.status", BuildFilterEndpoint("workorders.status", "New"))
        };
        
        foreach (var (description, endpoint) in testCases)
        {
            System.Console.WriteLine($"\n{description}:");
            System.Console.WriteLine($"Endpoint: {endpoint.Substring(0, Math.Min(100, endpoint.Length))}...");
            
            try
            {
                var response = await apiService.GetAsync<object>(endpoint);
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
                
                if (json.Contains("\"error\""))
                {
                    var errorStart = json.IndexOf("\"error\"");
                    var errorEnd = json.IndexOf("}", errorStart) + 1;
                    System.Console.WriteLine($"✗ Error: {json.Substring(errorStart, Math.Min(200, errorEnd - errorStart))}");
                }
                else
                {
                    System.Console.WriteLine($"✓ Success! Got {json.Length} chars");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }
    }
    
    private static async Task TestAllVariations(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing All Variations ---");
        
        // Test vendor_id filtering
        System.Console.WriteLine("\n=== Vendor ID Tests ===");
        await TestFilterVariations(apiService, "vendor_id", "5444");
        
        // Test status filtering
        System.Console.WriteLine("\n=== Status Tests ===");
        await TestFilterVariations(apiService, "status", "New");
        
        // Test date range
        System.Console.WriteLine("\n=== Date Range Tests ===");
        await TestDateRangeVariations(apiService);
    }
    
    private static async Task TestFilterVariations(IFexaApiService apiService, string field, string value)
    {
        var variations = new[]
        {
            // Direct query parameter
            $"/api/ev1/workorders?{field}={value}&start=0&limit=5",
            
            // With filters array - basic
            BuildFilterEndpoint(field, value),
            
            // With filters array - with prefix
            BuildFilterEndpoint($"workorders.{field}", value),
            
            // Special case for status - nested path
            field == "status" ? BuildFilterEndpoint("object_state.status.name", value) : null
        };
        
        foreach (var endpoint in variations.Where(e => e != null))
        {
            System.Console.WriteLine($"\nTesting: {endpoint!.Substring(0, Math.Min(80, endpoint.Length))}...");
            
            try
            {
                var response = await apiService.GetAsync<object>(endpoint);
                var json = JsonSerializer.Serialize(response);
                
                if (json.Contains("\"error\""))
                {
                    System.Console.WriteLine("✗ Error response");
                }
                else if (json.Contains("\"workorders\""))
                {
                    System.Console.WriteLine("✓ Success - got work orders");
                }
                else
                {
                    System.Console.WriteLine("? Unknown response format");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Exception: {ex.GetType().Name}");
            }
        }
    }
    
    private static async Task TestDateRangeVariations(IFexaApiService apiService)
    {
        var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "created_at",
                ["operator"] = "between",
                ["value"] = new[] { $"{startDate} 00:00:00", $"{endDate} 23:59:59" }
            }
        };
        
        var filterJson = JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        var endpoint = $"/api/ev1/workorders?filters={encodedFilter}&start=0&limit=5";
        
        System.Console.WriteLine($"\nDate range filter: {startDate} to {endDate}");
        
        try
        {
            var response = await apiService.GetAsync<object>(endpoint);
            var json = JsonSerializer.Serialize(response);
            
            if (json.Contains("\"error\""))
            {
                System.Console.WriteLine("✗ Error response");
            }
            else if (json.Contains("\"workorders\""))
            {
                System.Console.WriteLine("✓ Success - got work orders");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Exception: {ex.Message}");
        }
    }
    
    private static string BuildFilterEndpoint(string property, string value)
    {
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = property,
                ["value"] = value
            }
        };
        
        var filterJson = JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        return $"/api/ev1/workorders?filters={encodedFilter}&start=0&limit=5";
    }
    
    private static async Task TestPostWithBody(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing POST with Filters in Body ---");
        
        System.Console.Write("Enter field to filter (e.g., status, vendor_id, assigned_to): ");
        var field = System.Console.ReadLine();
        System.Console.Write("Enter value: ");
        var value = System.Console.ReadLine();
        
        var endpoint = "/api/ev1/workorders?start=0&limit=5";
        
        // Test different filter formats in body
        var filterFormats = new[]
        {
            new { filters = new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = field,
                    ["value"] = value
                }
            }},
            new { filters = new[]
            {
                new Dictionary<string, object>
                {
                    ["property"] = field,
                    ["operator"] = "equals",
                    ["value"] = value
                }
            }}
        };
        
        foreach (var body in filterFormats)
        {
            var json = JsonSerializer.Serialize(body);
            System.Console.WriteLine($"\nPOST to: {endpoint}");
            System.Console.WriteLine($"Body: {json}");
            
            try
            {
                var response = await apiService.PostAsync<object>(endpoint, body);
                var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
                
                if (responseJson.Contains("\"workorders\""))
                {
                    System.Console.WriteLine("✓ Success - got work orders");
                    // Count results
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
                        if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                        {
                            var count = parsed["workorders"].GetArrayLength();
                            System.Console.WriteLine($"✓ Found {count} work order(s)");
                        }
                    }
                    catch { }
                }
                else if (responseJson.Contains("\"error\""))
                {
                    System.Console.WriteLine("✗ Got error response");
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
                        if (parsed?.ContainsKey("error") == true)
                        {
                            System.Console.WriteLine($"   Error: {parsed["error"]}");
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }
    }
    
    private static async Task TestGetWithBody(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n--- Testing GET with Filters in Body ---");
        
        System.Console.Write("Enter field to filter (e.g., status, vendor_id, assigned_to): ");
        var field = System.Console.ReadLine();
        System.Console.Write("Enter value: ");
        var value = System.Console.ReadLine();
        
        var endpoint = "/api/ev1/workorders?start=0&limit=5";
        
        var filterBody = new { filters = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = field,
                ["value"] = value
            }
        }};
        
        var json = JsonSerializer.Serialize(filterBody);
        System.Console.WriteLine($"\nGET to: {endpoint}");
        System.Console.WriteLine($"Body: {json}");
        
        try
        {
            // Create GET request with body
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, endpoint);
            request.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var httpResponse = await apiService.SendAsync(request);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            
            System.Console.WriteLine($"Response Status: {httpResponse.StatusCode}");
            
            if (httpResponse.IsSuccessStatusCode)
            {
                if (responseContent.Contains("\"workorders\""))
                {
                    System.Console.WriteLine("✓ Success - got work orders");
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
                    if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                    {
                        var count = parsed["workorders"].GetArrayLength();
                        System.Console.WriteLine($"✓ Found {count} work order(s)");
                    }
                }
                else if (responseContent.Contains("\"error\""))
                {
                    System.Console.WriteLine("✗ Got error response");
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
                    if (parsed?.ContainsKey("error") == true)
                    {
                        System.Console.WriteLine($"   Error: {parsed["error"]}");
                    }
                }
            }
            else
            {
                System.Console.WriteLine($"✗ Request failed: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Exception: {ex.Message}");
        }
    }
    
    private static async Task QuickStatusFilterTest(IFexaApiService apiService)
    {
        System.Console.WriteLine("\n=== Quick Status Filter Test - All Variations ===");
        System.Console.WriteLine("Testing filtering for status='Action Required'\n");
        
        var testStatus = "Action Required";
        var testCases = new List<(string description, Func<Task>)>();
        
        // 1. Direct query parameters
        testCases.Add((
            "Direct query param: ?status=",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?status={testStatus}&start=0&limit=5")
        ));
        
        testCases.Add((
            "Direct nested: ?object_state.status.name=",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?object_state.status.name={testStatus}&start=0&limit=5")
        ));
        
        testCases.Add((
            "With prefix: ?workorders.object_state.status.name=",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?workorders.object_state.status.name={testStatus}&start=0&limit=5")
        ));
        
        // 2. Filters array in query
        var filter1 = new[] { new Dictionary<string, object> { ["property"] = "status", ["value"] = testStatus } };
        var encoded1 = HttpUtility.UrlEncode(JsonSerializer.Serialize(filter1));
        testCases.Add((
            "Filter array: property='status'",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?filters={encoded1}&start=0&limit=5")
        ));
        
        var filter2 = new[] { new Dictionary<string, object> { ["property"] = "object_state.status.name", ["value"] = testStatus } };
        var encoded2 = HttpUtility.UrlEncode(JsonSerializer.Serialize(filter2));
        testCases.Add((
            "Filter array: property='object_state.status.name'",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?filters={encoded2}&start=0&limit=5")
        ));
        
        var filter3 = new[] { new Dictionary<string, object> { ["property"] = "object_state.status.name", ["operator"] = "equals", ["value"] = testStatus } };
        var encoded3 = HttpUtility.UrlEncode(JsonSerializer.Serialize(filter3));
        testCases.Add((
            "Filter array with operator: property='object_state.status.name', operator='equals'",
            async () => await TestEndpoint(apiService, $"/api/ev1/workorders?filters={encoded3}&start=0&limit=5")
        ));
        
        // 3. POST with body
        testCases.Add((
            "POST with body: filters=[{property:'status', value:'Action Required'}]",
            async () => await TestPostEndpoint(apiService, "/api/ev1/workorders?start=0&limit=5", 
                new { filters = new[] { new Dictionary<string, object> { ["property"] = "status", ["value"] = testStatus } } })
        ));
        
        testCases.Add((
            "POST with body: filters=[{property:'object_state.status.name', value:'Action Required'}]",
            async () => await TestPostEndpoint(apiService, "/api/ev1/workorders?start=0&limit=5", 
                new { filters = new[] { new Dictionary<string, object> { ["property"] = "object_state.status.name", ["value"] = testStatus } } })
        ));
        
        // 4. GET with body
        testCases.Add((
            "GET with body: filters=[{property:'status', value:'Action Required'}]",
            async () => await TestGetWithBodyEndpoint(apiService, "/api/ev1/workorders?start=0&limit=5",
                new { filters = new[] { new Dictionary<string, object> { ["property"] = "status", ["value"] = testStatus } } })
        ));
        
        testCases.Add((
            "GET with body: filters=[{property:'object_state.status.name', value:'Action Required'}]",
            async () => await TestGetWithBodyEndpoint(apiService, "/api/ev1/workorders?start=0&limit=5",
                new { filters = new[] { new Dictionary<string, object> { ["property"] = "object_state.status.name", ["value"] = testStatus } } })
        ));
        
        // Execute all tests
        var results = new List<(string test, bool success, string result)>();
        
        foreach (var (description, test) in testCases)
        {
            System.Console.Write($"{description}... ");
            try
            {
                await test();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }
        
        System.Console.WriteLine("\n=== Summary ===");
        System.Console.WriteLine("Check the results above to see which approach works for filtering work orders by status.");
    }
    
    private static async Task TestEndpoint(IFexaApiService apiService, string endpoint)
    {
        try
        {
            var response = await apiService.GetAsync<object>(endpoint);
            var json = JsonSerializer.Serialize(response);
            
            if (json.Contains("\"workorders\":["))
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                {
                    var count = parsed["workorders"].GetArrayLength();
                    System.Console.WriteLine($"✓ Success - {count} work order(s)");
                    return;
                }
            }
            else if (json.Contains("\"error\""))
            {
                System.Console.WriteLine("✗ Error response");
                return;
            }
            
            System.Console.WriteLine("? Unknown response");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Exception: {ex.GetType().Name}");
        }
    }
    
    private static async Task TestPostEndpoint(IFexaApiService apiService, string endpoint, object body)
    {
        try
        {
            var response = await apiService.PostAsync<object>(endpoint, body);
            var json = JsonSerializer.Serialize(response);
            
            if (json.Contains("\"workorders\":["))
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                {
                    var count = parsed["workorders"].GetArrayLength();
                    System.Console.WriteLine($"✓ Success - {count} work order(s)");
                    return;
                }
            }
            else if (json.Contains("\"error\""))
            {
                System.Console.WriteLine("✗ Error response");
                return;
            }
            
            System.Console.WriteLine("? Unknown response");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Exception: {ex.GetType().Name}");
        }
    }
    
    private static async Task TestGetWithBodyEndpoint(IFexaApiService apiService, string endpoint, object body)
    {
        try
        {
            var json = JsonSerializer.Serialize(body);
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, endpoint);
            request.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var httpResponse = await apiService.SendAsync(request);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            
            if (responseContent.Contains("\"workorders\":["))
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
                if (parsed?.ContainsKey("workorders") == true && parsed["workorders"].ValueKind == JsonValueKind.Array)
                {
                    var count = parsed["workorders"].GetArrayLength();
                    System.Console.WriteLine($"✓ Success - {count} work order(s)");
                    return;
                }
            }
            else if (responseContent.Contains("\"error\""))
            {
                System.Console.WriteLine("✗ Error response");
                return;
            }
            
            System.Console.WriteLine("? Unknown response");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Exception: {ex.GetType().Name}");
        }
    }
}