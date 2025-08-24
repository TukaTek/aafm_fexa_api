using Microsoft.Extensions.DependencyInjection;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class TestStatusUpdateFix
{
    public static async Task TestDirectStatusUpdate(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Testing Direct Status Update Fix ===\n");
        
        using var scope = services.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<IFexaApiService>();
        
        System.Console.Write("Enter Work Order ID to test: ");
        var workOrderIdStr = System.Console.ReadLine();
        if (!int.TryParse(workOrderIdStr, out var workOrderId))
        {
            System.Console.WriteLine("Invalid work order ID");
            return;
        }
        
        System.Console.Write("Enter target Status ID: ");
        var statusIdStr = System.Console.ReadLine();
        if (!int.TryParse(statusIdStr, out var statusId))
        {
            System.Console.WriteLine("Invalid status ID");
            return;
        }
        
        System.Console.WriteLine($"\nTesting status update for WO {workOrderId} to status {statusId}");
        
        // Test the exact endpoint format that works in Postman
        var endpoint = $"/api/ev1/workorders/{workOrderId}/update_status/{statusId}";
        System.Console.WriteLine($"Testing endpoint: PUT {endpoint}");
        
        try
        {
            // Try with no body first (as Postman shows)
            System.Console.WriteLine("Attempt 1: No body");
            var response1 = await apiService.PutAsync<dynamic>(endpoint, null);
            System.Console.WriteLine($"Response: {response1}");
            
            // Check if it's an error response
            var responseStr = response1?.ToString() ?? "";
            if (responseStr.Contains("error"))
            {
                System.Console.WriteLine("❌ Got error response");
                
                // Try with empty object body
                System.Console.WriteLine("\nAttempt 2: Empty object body");
                var response2 = await apiService.PutAsync<dynamic>(endpoint, new { });
                System.Console.WriteLine($"Response: {response2}");
                
                responseStr = response2?.ToString() ?? "";
                if (!responseStr.Contains("error"))
                {
                    System.Console.WriteLine("✅ Success with empty object body!");
                }
            }
            else
            {
                System.Console.WriteLine("✅ Success with no body!");
            }
            
            // Verify the status actually changed
            System.Console.WriteLine("\nVerifying status change...");
            var getEndpoint = $"/api/ev1/workorders/{workOrderId}";
            var workOrderResponse = await apiService.GetAsync<dynamic>(getEndpoint);
            System.Console.WriteLine($"Current work order status check: {workOrderResponse}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}