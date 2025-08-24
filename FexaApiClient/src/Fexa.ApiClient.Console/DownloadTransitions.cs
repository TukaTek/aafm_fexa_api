using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Console;

public static class DownloadTransitions
{
    public static async Task DownloadAllTransitionsToFile(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        
        System.Console.WriteLine("Fetching all transitions from API...");
        
        try
        {
            // Get all transitions
            var allTransitions = await transitionService.GetAllTransitionsAsync();
            
            System.Console.WriteLine($"Downloaded {allTransitions.Count} transitions");
            
            // Create the full JSON structure with all transitions
            var jsonData = new
            {
                transitions = allTransitions,
                count = allTransitions.Count,
                downloaded_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };
            
            // Serialize to JSON with pretty formatting
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var jsonString = JsonSerializer.Serialize(jsonData, jsonOptions);
            
            // Save to file in the root directory (parent of FexaApiClient)
            var currentDirectory = Directory.GetCurrentDirectory();
            var rootDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
            var filePath = Path.Combine(rootDirectory, "transitions.json");
            
            await File.WriteAllTextAsync(filePath, jsonString);
            
            System.Console.WriteLine($"Successfully saved transitions to: {filePath}");
            System.Console.WriteLine($"File size: {new FileInfo(filePath).Length / 1024} KB");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error downloading transitions: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}