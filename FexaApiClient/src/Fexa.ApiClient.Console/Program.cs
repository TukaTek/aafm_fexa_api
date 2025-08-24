using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Extensions;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using Fexa.ApiClient.Exceptions;

namespace Fexa.ApiClient.Console;

class Program
{
    static async Task Main(string[] args)
    {
        // Check if we should run the download program
        if (args.Length > 0 && args[0] == "download-transitions")
        {
            await DownloadTransitionsProgram.RunDownload(args);
            return;
        }
        
        var host = CreateHostBuilder(args).Build();
        
        try
        {
            var menuSystem = new MenuSystem(host.Services);
            await menuSystem.RunAsync();
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Application error occurred");
            System.Console.WriteLine($"\nFatal error: {ex.Message}");
        }
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddUserSecrets<Program>(optional: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Add Fexa API client with configuration
                services.AddFexaApiClient(context.Configuration);
                
                // Configure logging
                services.AddLogging(config =>
                {
                    config.ClearProviders();
                    config.AddConsole();
                    config.SetMinimumLevel(LogLevel.Warning); // Reduce console noise
                    config.AddDebug();
                });
            });
    
    // Legacy method - kept for reference
    static async Task RunApiTests(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var userService = services.GetRequiredService<IUserService>();
        
        logger.LogInformation("Starting Fexa API tests...");
        
        try
        {
            // Test 1: Create a new user
            logger.LogInformation("Test 1: Creating a new user...");
            var createUserRequest = new CreateUserRequest
            {
                Email = $"test.user.{Guid.NewGuid():N}@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            var createdUser = await userService.CreateUserAsync(createUserRequest);
            logger.LogInformation("User created successfully: {UserId} - {Email}", 
                createdUser.Id, createdUser.Email);
            
            // Test 2: Get user by ID
            logger.LogInformation("Test 2: Getting user by ID...");
            var retrievedUser = await userService.GetUserAsync(createdUser.Id);
            logger.LogInformation("User retrieved: {FirstName} {LastName}", 
                retrievedUser.FirstName, retrievedUser.LastName);
            
            // Test 3: Update user
            logger.LogInformation("Test 3: Updating user...");
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "Name"
            };
            
            var updatedUser = await userService.UpdateUserAsync(createdUser.Id, updateRequest);
            logger.LogInformation("User updated: {FirstName} {LastName}", 
                updatedUser.FirstName, updatedUser.LastName);
            
            // Test 4: Get all users with pagination
            logger.LogInformation("Test 4: Getting users with pagination...");
            var queryParams = new QueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = "createdAt",
                SortDescending = true
            };
            
            var pagedUsers = await userService.GetUsersAsync(queryParams);
            logger.LogInformation("Retrieved {Count} users (Page {Page} of {TotalPages})", 
                pagedUsers.Data?.Count ?? 0, pagedUsers.Page, pagedUsers.TotalPages);
            
            // Test 4b: Demonstrate Fexa filtering
            logger.LogInformation("Test 4b: Testing Fexa filters...");
            
            // Example 1: Simple filter by vendor ID
            var vendorFilterParams = QueryParameters.Create()
                .WithFilters(filters => filters
                    .WhereVendorId(25));
            
            logger.LogInformation("Filter JSON: {FilterJson}", 
                vendorFilterParams.Filters != null ? FilterBuilder.Create().AddFilter(vendorFilterParams.Filters[0]).ToJson() : "none");
            
            // Example 2: Multiple filters (workorder and vendor)
            var multiFilterParams = QueryParameters.Create()
                .WithFilters(filters => filters
                    .WhereWorkOrderId(1)
                    .WhereVendorId(25));
            
            // Example 3: Using IN operator
            var inFilterParams = QueryParameters.Create()
                .WithFilters(filters => filters
                    .WhereWorkOrderIds(116, 117));
            
            // Example 4: Date range filter
            var dateFilterParams = QueryParameters.Create()
                .WithFilters(filters => filters
                    .WhereDateBetween("invoices.created_at", 
                        DateTime.Now.AddDays(-30), 
                        DateTime.Now));
            
            // Test 5: Delete user
            logger.LogInformation("Test 5: Deleting user...");
            await userService.DeleteUserAsync(createdUser.Id);
            logger.LogInformation("User deleted successfully");
            
            // Test 6: Error handling - try to get deleted user
            logger.LogInformation("Test 6: Testing error handling...");
            try
            {
                await userService.GetUserAsync(createdUser.Id);
            }
            catch (FexaApiException ex)
            {
                logger.LogWarning("Expected error occurred: {Message} (Status: {StatusCode})", 
                    ex.Message, ex.StatusCode);
            }
            
            logger.LogInformation("All tests completed successfully!");
        }
        catch (FexaAuthenticationException ex)
        {
            logger.LogError("Authentication failed: {Message}", ex.Message);
            logger.LogError("Please check your Client ID and Client Secret in appsettings.json or user secrets");
        }
        catch (FexaRateLimitException ex)
        {
            logger.LogError("Rate limit exceeded: {Message}. Retry after {Seconds} seconds", 
                ex.Message, ex.RetryAfterSeconds);
        }
        catch (FexaValidationException ex)
        {
            logger.LogError("Validation failed: {Message}", ex.Message);
            if (ex.ValidationErrors != null)
            {
                foreach (var error in ex.ValidationErrors)
                {
                    logger.LogError("  - {Field}: {Errors}", error.Key, string.Join(", ", error.Value));
                }
            }
        }
        catch (FexaApiException ex)
        {
            logger.LogError("API error: {Message} (Status: {StatusCode}, RequestId: {RequestId})", 
                ex.Message, ex.StatusCode, ex.RequestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred");
        }
    }
}