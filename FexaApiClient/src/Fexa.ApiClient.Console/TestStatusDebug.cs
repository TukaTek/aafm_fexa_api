using Microsoft.Extensions.DependencyInjection;
using Fexa.ApiClient.Services;
using System.Text.Json;

namespace Fexa.ApiClient.Console;

public static class TestStatusDebug
{
    public static async Task DebugStatuses(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Debug Work Order Statuses ===\n");
        
        using var scope = services.CreateScope();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        
        try
        {
            System.Console.WriteLine("Fetching Work Order statuses...");
            var statuses = await transitionService.GetWorkOrderStatusesAsync();
            
            System.Console.WriteLine($"\n✅ Found {statuses.Count} unique Work Order statuses\n");
            
            if (statuses.Any())
            {
                System.Console.WriteLine("First 10 statuses:");
                foreach (var status in statuses.Take(10))
                {
                    System.Console.WriteLine($"  ID: {status.Id}, Name: {status.Name}");
                }
                
                // Check for common ones
                var commonNames = new[] { "New", "Action Required", "In Progress", "Scheduled", "Completed", "Cancelled", "Pending" };
                System.Console.WriteLine("\nSearching for common status names:");
                foreach (var name in commonNames)
                {
                    var found = statuses.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (found.Any())
                    {
                        System.Console.WriteLine($"  '{name}' found in: {string.Join(", ", found.Select(s => $"'{s.Name}'"))}");
                    }
                    else
                    {
                        System.Console.WriteLine($"  '{name}' - NOT FOUND");
                    }
                }
                
                // Also fetch raw transitions to compare
                System.Console.WriteLine("\n--- Comparing with raw transitions ---");
                var transitions = await transitionService.GetTransitionsByTypeAsync("Assignment");
                System.Console.WriteLine($"Found {transitions.Count} Assignment transitions");
                
                if (transitions.Any())
                {
                    System.Console.WriteLine("\nFirst transition details:");
                    var first = transitions.First();
                    System.Console.WriteLine($"  Transition Name: {first.Name}");
                    System.Console.WriteLine($"  From Status: {first.FromStatus?.Name} (ID: {first.FromStatusId})");
                    System.Console.WriteLine($"  To Status: {first.ToStatus?.Name} (ID: {first.ToStatusId})");
                    System.Console.WriteLine($"  Object Type: {first.WorkflowObjectType}");
                }
            }
            else
            {
                System.Console.WriteLine("❌ No statuses found!");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.WriteLine($"Stack: {ex.StackTrace}");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey(true);
    }
}