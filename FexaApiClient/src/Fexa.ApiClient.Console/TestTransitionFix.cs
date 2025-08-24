using Microsoft.Extensions.DependencyInjection;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class TestTransitionFix
{
    public static async Task TestActionRequiredTransitions(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Testing Action Required → Awaiting Client Transition ===\n");
        
        using var scope = services.CreateScope();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        
        try
        {
            // Get all transitions
            System.Console.WriteLine("Loading all transitions...");
            var allTransitions = await transitionService.GetAllTransitionsAsync();
            
            // Find Work Order transitions from Action Required (ID: 87)
            var actionRequiredId = 87;
            var workOrderTransitions = allTransitions
                .Where(t => t.WorkflowObjectType == "Work Order" && t.FromStatusId == actionRequiredId)
                .ToList();
            
            System.Console.WriteLine($"\n✅ Found {workOrderTransitions.Count} Work Order transitions from 'Action Required' (ID: {actionRequiredId})");
            System.Console.WriteLine("\nAvailable transitions:");
            
            foreach (var transition in workOrderTransitions.OrderBy(t => t.ToStatus?.Name))
            {
                var checkmark = transition.ToStatus?.Name == "Awaiting Client" ? " ✓" : "";
                System.Console.WriteLine($"  - {transition.ToStatus?.Name} (ID: {transition.ToStatusId}){checkmark}");
            }
            
            // Check if Awaiting Client is in the list
            var hasAwaitingClient = workOrderTransitions.Any(t => t.ToStatus?.Name == "Awaiting Client");
            
            if (hasAwaitingClient)
            {
                System.Console.WriteLine("\n✅ SUCCESS: 'Awaiting Client' is properly detected as a valid transition!");
            }
            else
            {
                System.Console.WriteLine("\n❌ FAILED: 'Awaiting Client' was NOT found in valid transitions!");
            }
            
            // Also show the count by workflow type
            System.Console.WriteLine("\n=== Transition Statistics ===");
            var byType = allTransitions.GroupBy(t => t.WorkflowObjectType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            
            foreach (var type in byType)
            {
                System.Console.WriteLine($"  - {type.Type}: {type.Count} transitions");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }
}