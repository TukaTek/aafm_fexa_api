using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Extensions;

namespace Fexa.ApiClient.Console;

public class DownloadTransitionsProgram
{
    public static async Task RunDownload(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .Build();

        // Setup DI container
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Add Fexa API client
        services.AddFexaApiClient(options =>
        {
            configuration.GetSection("FexaApi").Bind(options);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
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
            
            // Save to file in the root directory (aafm_fexa_api)
            var currentDirectory = Directory.GetCurrentDirectory();
            // Navigate up from FexaApiClient/src/Fexa.ApiClient.Console to aafm_fexa_api
            var rootDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", "..", ".."));
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
            Environment.Exit(1);
        }
    }
}