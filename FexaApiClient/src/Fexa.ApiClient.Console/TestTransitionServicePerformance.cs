using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class TestTransitionServicePerformance
{
    public static async Task RunTransitionServiceTests(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Transition Service Test ===\n");
        
        using var scope = services.CreateScope();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ITransitionService>>();
        
        try
        {
            // Test 1: Get Work Order Statuses (uses cache)
            System.Console.WriteLine("Test 1: Getting Work Order statuses...");
            var workOrderStatuses = await transitionService.GetWorkOrderStatusesAsync();
            
            System.Console.WriteLine($"✅ Found {workOrderStatuses.Count} Work Order statuses");
            
            // Show common statuses
            var commonStatusNames = new[] { "New", "Action Required", "In Progress", "Scheduled", "Completed", "Cancelled" };
            var commonStatuses = workOrderStatuses
                .Where(s => commonStatusNames.Any(name => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(s => s.Name)
                .ToList();
            
            if (commonStatuses.Any())
            {
                System.Console.WriteLine("\n📋 Common Work Order statuses:");
                foreach (var status in commonStatuses)
                {
                    System.Console.WriteLine($"   - {status.Name} (ID: {status.Id})");
                }
            }
            
            // Test 2: Get transitions for a specific status (from cache)
            if (workOrderStatuses.Any())
            {
                var testStatus = workOrderStatuses.First();
                System.Console.WriteLine($"\nTest 2: Getting transitions for status '{testStatus.Name}' (ID: {testStatus.Id})...");
                
                var transitionsFrom = await transitionService.GetTransitionsFromStatusAsync(testStatus.Id);
                var transitionsTo = await transitionService.GetTransitionsToStatusAsync(testStatus.Id);
                
                var workOrderTransitionsFrom = transitionsFrom.Where(t => t.WorkflowObjectType == "Work Order").ToList();
                var workOrderTransitionsTo = transitionsTo.Where(t => t.WorkflowObjectType == "Work Order").ToList();
                
                System.Console.WriteLine($"✅ Found {workOrderTransitionsFrom.Count} transitions FROM this status");
                System.Console.WriteLine($"✅ Found {workOrderTransitionsTo.Count} transitions TO this status");
                
                if (workOrderTransitionsFrom.Any())
                {
                    System.Console.WriteLine($"\n➡️  Can transition FROM '{testStatus.Name}' to:");
                    foreach (var t in workOrderTransitionsFrom.Take(3))
                    {
                        System.Console.WriteLine($"   - {t.ToStatus?.Name} (via '{t.Name}')");
                    }
                }
                
                if (workOrderTransitionsTo.Any())
                {
                    System.Console.WriteLine($"\n⬅️  Can transition TO '{testStatus.Name}' from:");
                    foreach (var t in workOrderTransitionsTo.Take(3))
                    {
                        System.Console.WriteLine($"   - {t.FromStatus?.Name} (via '{t.Name}')");
                    }
                }
            }
            
            // Test 3: Verify caching is working
            System.Console.WriteLine("\nTest 3: Testing cache efficiency...");
            var start = DateTime.UtcNow;
            await transitionService.GetWorkOrderStatusesAsync();
            var elapsed1 = DateTime.UtcNow - start;
            
            start = DateTime.UtcNow;
            await transitionService.GetWorkOrderStatusesAsync();
            var elapsed2 = DateTime.UtcNow - start;
            
            System.Console.WriteLine($"✅ First call: {elapsed1.TotalMilliseconds:F0}ms");
            System.Console.WriteLine($"✅ Second call (cached): {elapsed2.TotalMilliseconds:F0}ms");
            
            if (elapsed2 < elapsed1)
            {
                System.Console.WriteLine("✅ Cache is working! Second call was faster.");
            }
            
            System.Console.WriteLine("\n✨ All tests completed successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error: {ex.Message}");
            logger.LogError(ex, "Test failed");
        }
        
        System.Console.WriteLine("\nPress any key to return to main menu...");
        System.Console.ReadKey(true);
    }
}