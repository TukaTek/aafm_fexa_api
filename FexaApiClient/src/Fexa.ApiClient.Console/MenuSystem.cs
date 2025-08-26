using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Exceptions;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Console;

public class MenuSystem
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MenuSystem> _logger;
    private readonly IConfiguration _configuration;
    private bool _exitRequested = false;
    
    public MenuSystem(IServiceProvider services)
    {
        _services = services;
        _logger = services.GetRequiredService<ILogger<MenuSystem>>();
        _configuration = services.GetRequiredService<IConfiguration>();
    }
    
    public async Task RunAsync()
    {
        System.Console.Clear();
        ShowWelcome();
        
        while (!_exitRequested)
        {
            ShowMainMenu();
            var choice = System.Console.ReadLine()?.Trim();
            
            try
            {
                await ProcessMainMenuChoice(choice);
            }
            catch (Exception ex)
            {
                ShowError($"An error occurred: {ex.Message}");
            }
            
            if (!_exitRequested)
            {
                ShowPressAnyKey();
            }
        }
        
        ShowGoodbye();
    }
    
    private void ShowWelcome()
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║        Fexa API Client Test Console        ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.ResetColor();
        System.Console.WriteLine();
    }
    
    private void ShowMainMenu()
    {
        System.Console.WriteLine("\n=== Main Menu ===");
        System.Console.WriteLine("1. Test Fexa API Connection");
        System.Console.WriteLine("2. Test Visit Service");
        System.Console.WriteLine("3. Test Work Order Service");
        System.Console.WriteLine("4. Debug Work Order API");
        System.Console.WriteLine("5. Get Workflow Statuses & Transitions");
        System.Console.WriteLine("6. Test Transition Service (Cache & Performance)");
        System.Console.WriteLine("7. Debug Work Order Statuses");
        System.Console.WriteLine("8. Update Work Order Status");
        System.Console.WriteLine("9. Download All Transitions to JSON File");
        System.Console.WriteLine("10. Test Action Required Transitions (Fix Verification)");
        System.Console.WriteLine("11. Test Direct Status Update (Debug)");
        System.Console.WriteLine("12. Test Note Service");
        System.Console.WriteLine("13. Test Client PO Filter");
        System.Console.WriteLine("14. Test Client Service");
        System.Console.WriteLine("15. Test Vendor Service");
        System.Console.WriteLine("16. Test Document Upload Service");
        System.Console.WriteLine("17. Test Work Order Category Service");
        System.Console.WriteLine("0. Exit");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
    }
    
    private async Task ProcessMainMenuChoice(string? choice)
    {
        switch (choice)
        {
            case "1":
                await TestConnection();
                break;
            case "2":
                await TestVisitService();
                break;
            case "3":
                await TestWorkOrderService();
                break;
            case "4":
                await TestWorkOrderDebug.DebugWorkOrderApiCall(_services);
                break;
            case "5":
                await TestTransitionService();
                break;
            case "6":
                await TestTransitionServicePerformance.RunTransitionServiceTests(_services);
                break;
            case "7":
                await TestStatusDebug.DebugStatuses(_services);
                break;
            case "8":
                await UpdateWorkOrderStatus();
                break;
            case "9":
                await DownloadTransitions.DownloadAllTransitionsToFile(_services);
                break;
            case "10":
                await TestTransitionFix.TestActionRequiredTransitions(_services);
                break;
            case "11":
                await TestStatusUpdateFix.TestDirectStatusUpdate(_services);
                break;
            case "12":
                await TestNoteService.RunNoteServiceTests(_services);
                break;
            case "13":
                await TestClientPOFilter.RunAsync(_configuration);
                break;
            case "14":
                await TestClientService();
                break;
            case "15":
                await TestVendorService();
                break;
            case "16":
                await TestDocumentUpload.RunDocumentUploadTests(_services);
                break;
            case "17":
                await TestWorkOrderCategoryService();
                break;
            case "0":
                _exitRequested = true;
                break;
            default:
                ShowError("Invalid choice. Please try again.");
                break;
        }
    }
    
    private async Task TestConnection()
    {
        System.Console.Clear();
        ShowHeader("Test Fexa API Connection");
        
        // ITokenService is singleton, so no scope needed
        var tokenService = _services.GetRequiredService<ITokenService>();
        
        ShowInfo("Testing Fexa API connection...");
        
        try
        {
            // Try to get an access token
            ShowInfo("Acquiring OAuth 2.0 access token...");
            var tokenResponse = await tokenService.RefreshTokenAsync();
            
            ShowSuccess("Authentication successful!");
            System.Console.WriteLine($"Access token acquired (first 20 chars): {tokenResponse.AccessToken.Substring(0, Math.Min(20, tokenResponse.AccessToken.Length))}...");
            System.Console.WriteLine($"Token type: {tokenResponse.TokenType}");
            System.Console.WriteLine($"Expires in: {tokenResponse.ExpiresIn} seconds");
            System.Console.WriteLine($"Expires at: {tokenResponse.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
            
            // Try a simple API call - create scope for scoped service
            using (var scope = _services.CreateScope())
            {
                var apiService = scope.ServiceProvider.GetRequiredService<IFexaApiService>();
                ShowInfo("\nMaking test API call to /api/ev1/users...");
                
                var response = await apiService.GetAsync<object>("/api/ev1/users?start=0&limit=1");
                
                ShowSuccess("API call successful!");
                ShowSuccess("\nConnection test completed successfully.");
            }
        }
        catch (FexaAuthenticationException ex)
        {
            ShowError($"Authentication failed: {ex.Message}");
            ShowError("Please check your Client ID and Client Secret in appsettings.Development.json");
            if (!string.IsNullOrEmpty(ex.ResponseContent))
            {
                ShowError($"Response: {ex.ResponseContent}");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Connection test failed: {ex.Message}");
            ShowError($"Error type: {ex.GetType().Name}");
        }
    }
    
    private async Task TestWorkOrderService()
    {
        System.Console.Clear();
        ShowHeader("Test Work Order Service");
        
        // Create a scope to resolve scoped services
        using var scope = _services.CreateScope();
        var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();
        
        System.Console.WriteLine("\n=== Work Order Service Menu ===");
        System.Console.WriteLine("1. Get work orders (single page)");
        System.Console.WriteLine("2. Get ALL work orders (multiple pages)");
        System.Console.WriteLine("3. Get work order by ID");
        System.Console.WriteLine("4. Get work orders by status (single page)");
        System.Console.WriteLine("5. Get ALL work orders by status (multiple pages)");
        System.Console.WriteLine("6. Get work orders by vendor");
        System.Console.WriteLine("7. Get work orders by client");
        System.Console.WriteLine("8. Get work orders by technician");
        System.Console.WriteLine("9. Get work orders by date range");
        System.Console.WriteLine("0. Back to main menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
        
        var choice = System.Console.ReadLine()?.Trim();
        
        try
        {
            switch (choice)
            {
                case "1":
                    await GetWorkOrdersSinglePage(workOrderService);
                    break;
                case "2":
                    await GetAllWorkOrders(workOrderService);
                    break;
                case "3":
                    await GetWorkOrderById(workOrderService);
                    break;
                case "4":
                    await GetWorkOrdersByStatusSinglePage(workOrderService);
                    break;
                case "5":
                    await GetAllWorkOrdersByStatus(workOrderService);
                    break;
                case "6":
                    await GetWorkOrdersByVendor(workOrderService);
                    break;
                case "7":
                    await GetWorkOrdersByClient(workOrderService);
                    break;
                case "8":
                    await GetWorkOrdersByTechnician(workOrderService);
                    break;
                case "9":
                    await GetWorkOrdersByDateRange(workOrderService);
                    break;
                case "0":
                    return;
                default:
                    ShowError("Invalid choice.");
                    break;
            }
        }
        catch (FexaApiException ex)
        {
            ShowError($"API Error: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ResponseContent))
            {
                ShowError($"Response: {ex.ResponseContent}");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
    
    private async Task GetWorkOrdersSinglePage(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter page size (default 20, max 100): ");
        var limitStr = System.Console.ReadLine();
        var limit = string.IsNullOrWhiteSpace(limitStr) ? 20 : Math.Min(100, int.Parse(limitStr));
        
        System.Console.Write("Enter page number (default 1): ");
        var pageStr = System.Console.ReadLine();
        var page = string.IsNullOrWhiteSpace(pageStr) ? 1 : int.Parse(pageStr);
        
        ShowInfo($"Fetching page {page} with {limit} work orders...");
        
        var parameters = new QueryParameters
        {
            Start = (page - 1) * limit,
            Limit = limit
        };
        
        var response = await workOrderService.GetWorkOrdersAsync(parameters);
        
        ShowSuccess($"Found {response.TotalCount} total work orders. Page {page} showing {response.Data?.Count() ?? 0}:");
        
        if (response.Data != null)
        {
            foreach (var workOrder in response.Data)
            {
                System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                if (!string.IsNullOrWhiteSpace(workOrder.ClientName))
                {
                    System.Console.WriteLine($"    Client: {workOrder.ClientName}");
                }
                if (workOrder.NextVisit.HasValue)
                {
                    System.Console.WriteLine($"    Next Visit: {workOrder.NextVisit:yyyy-MM-dd}");
                }
            }
        }
    }
    
    private async Task GetAllWorkOrders(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter max pages to fetch (default 5, max 10): ");
        var maxPagesStr = System.Console.ReadLine();
        var maxPages = string.IsNullOrWhiteSpace(maxPagesStr) ? 5 : Math.Min(10, int.Parse(maxPagesStr));
        
        ShowInfo($"Fetching ALL work orders (up to {maxPages} pages of 100 each)...");
        ShowInfo("This may take a moment...");
        
        var allWorkOrders = await workOrderService.GetAllWorkOrdersAsync(null, maxPages);
        
        ShowSuccess($"Fetched {allWorkOrders.Count} total work orders");
        
        // Group by status for summary
        var statusGroups = allWorkOrders.GroupBy(w => w.Status)
            .OrderByDescending(g => g.Count())
            .ToList();
        
        System.Console.WriteLine("\nWork Orders by Status:");
        foreach (var group in statusGroups)
        {
            System.Console.WriteLine($"  - {group.Key}: {group.Count()}");
        }
        
        System.Console.Write("\nShow all work orders? (y/n): ");
        if (System.Console.ReadLine()?.ToLower() == "y")
        {
            foreach (var workOrder in allWorkOrders.Take(500)) // Limit display to 500
            {
                System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                if (!string.IsNullOrWhiteSpace(workOrder.Description))
                {
                    var desc = workOrder.Description.Length > 50 
                        ? workOrder.Description.Substring(0, 47) + "..." 
                        : workOrder.Description;
                    System.Console.WriteLine($"    {desc}");
                }
            }
            
            if (allWorkOrders.Count > 500)
            {
                System.Console.WriteLine($"\n... and {allWorkOrders.Count - 500} more");
            }
        }
    }
    
    private async Task GetWorkOrderById(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter Work Order ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            ShowInfo($"Fetching work order {workOrderId}...");
            
            var workOrder = await workOrderService.GetWorkOrderAsync(workOrderId);
            
            ShowSuccess($"Work Order #{workOrder.Id}");
            System.Console.WriteLine($"  Status: {workOrder.Status}");
            System.Console.WriteLine($"  Priority ID: {workOrder.PriorityId?.ToString() ?? "N/A"}");
            if (!string.IsNullOrWhiteSpace(workOrder.Description))
            {
                System.Console.WriteLine($"  Description: {workOrder.Description}");
            }
            if (!string.IsNullOrWhiteSpace(workOrder.ClientName))
            {
                System.Console.WriteLine($"  Client: {workOrder.ClientName} (ID: {workOrder.ClientId})");
            }
            if (!string.IsNullOrWhiteSpace(workOrder.VendorName))
            {
                System.Console.WriteLine($"  Vendor: {workOrder.VendorName} (ID: {workOrder.AssignedTo})");
            }
            if (!string.IsNullOrWhiteSpace(workOrder.TechnicianName))
            {
                System.Console.WriteLine($"  Technician: {workOrder.TechnicianName} (ID: {workOrder.TechnicianId})");
            }
            if (!string.IsNullOrWhiteSpace(workOrder.StoreName))
            {
                System.Console.WriteLine($"  Store: {workOrder.StoreName}");
                System.Console.WriteLine($"  Address: {workOrder.StoreAddress}, {workOrder.StoreCity}, {workOrder.StoreState} {workOrder.StoreZip}");
            }
            if (workOrder.NextVisit.HasValue)
            {
                System.Console.WriteLine($"  Next Visit: {workOrder.NextVisit:yyyy-MM-dd HH:mm}");
            }
            if (workOrder.DateCompleted.HasValue)
            {
                System.Console.WriteLine($"  Completed: {workOrder.DateCompleted:yyyy-MM-dd HH:mm}");
            }
            if (workOrder.TotalAmount.HasValue)
            {
                System.Console.WriteLine($"  Total Amount: ${workOrder.TotalAmount:N2}");
            }
        }
        else
        {
            ShowError("Invalid work order ID.");
        }
    }
    
    private async Task GetWorkOrdersByStatusSinglePage(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter status (e.g., New, Action Required, In Progress, Completed): ");
        var status = System.Console.ReadLine()?.Trim();
        
        if (!string.IsNullOrEmpty(status))
        {
            ShowInfo($"Fetching work orders with status '{status}'...");
            
            var response = await workOrderService.GetWorkOrdersByStatusAsync(status);
            
            ShowSuccess($"Found {response.TotalCount} work orders with status '{status}' (showing first {Math.Min(response.Data?.Count ?? 0, 10)}):");
            
            if (response.Data != null)
            {
                // Show how many actually match the requested status
                var matchingCount = response.Data.Count(w => w.Status == status);
                if (matchingCount != response.Data.Count)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"⚠️  Note: API returned {response.Data.Count} work orders but only {matchingCount} have status '{status}'");
                    System.Console.WriteLine("   The API may not be filtering correctly on the server side.");
                    System.Console.ResetColor();
                }
                
                foreach (var workOrder in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - WO #{workOrder.Id}: {workOrder.Description ?? "No description"}");
                    System.Console.WriteLine($"    Status: {workOrder.Status}");
                    System.Console.WriteLine($"    Priority ID: {workOrder.PriorityId}");
                }
            }
        }
        else
        {
            ShowError("Status cannot be empty.");
        }
    }
    
    private async Task GetAllWorkOrdersByStatus(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter status (e.g., New, Action Required, In Progress, Completed): ");
        var status = System.Console.ReadLine()?.Trim();
        
        if (!string.IsNullOrEmpty(status))
        {
            System.Console.Write("Enter max pages to fetch (default 5, max 10): ");
            var maxPagesStr = System.Console.ReadLine();
            var maxPages = string.IsNullOrWhiteSpace(maxPagesStr) ? 5 : Math.Min(10, int.Parse(maxPagesStr));
            
            ShowInfo($"Fetching ALL work orders with status '{status}' (up to {maxPages} pages)...");
            ShowInfo("This may take a moment...");
            
            var allWorkOrders = await workOrderService.GetAllWorkOrdersByStatusAsync(status, null, maxPages);
            
            ShowSuccess($"Fetched {allWorkOrders.Count} work orders with status '{status}'");
            
            // Show first 20 with details
            System.Console.WriteLine("\nFirst 20 work orders:");
            foreach (var workOrder in allWorkOrders.Take(20))
            {
                System.Console.WriteLine($"  - WO #{workOrder.Id}");
                if (!string.IsNullOrWhiteSpace(workOrder.Description))
                {
                    var desc = workOrder.Description.Length > 60 
                        ? workOrder.Description.Substring(0, 57) + "..." 
                        : workOrder.Description;
                    System.Console.WriteLine($"    Description: {desc}");
                }
                if (!string.IsNullOrWhiteSpace(workOrder.ClientName))
                {
                    System.Console.WriteLine($"    Client: {workOrder.ClientName}");
                }
                if (workOrder.NextVisit.HasValue)
                {
                    System.Console.WriteLine($"    Next Visit: {workOrder.NextVisit:yyyy-MM-dd}");
                }
            }
            
            if (allWorkOrders.Count > 20)
            {
                System.Console.WriteLine($"\n... and {allWorkOrders.Count - 20} more work orders with status '{status}'");
                
                System.Console.Write("\nShow all IDs? (y/n): ");
                if (System.Console.ReadLine()?.ToLower() == "y")
                {
                    System.Console.WriteLine("\nAll Work Order IDs:");
                    var ids = string.Join(", ", allWorkOrders.Select(w => w.Id));
                    System.Console.WriteLine(ids);
                }
            }
        }
        else
        {
            ShowError("Status cannot be empty.");
        }
    }
    
    private async Task GetWorkOrdersByVendor(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter Vendor ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var vendorId))
        {
            ShowInfo($"Fetching work orders for vendor {vendorId}...");
            
            var response = await workOrderService.GetWorkOrdersByVendorAsync(vendorId);
            
            ShowSuccess($"Found {response.TotalCount} work orders for vendor {vendorId}:");
            
            if (response.Data != null)
            {
                foreach (var workOrder in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                    if (!string.IsNullOrWhiteSpace(workOrder.VendorName))
                    {
                        System.Console.WriteLine($"    Vendor: {workOrder.VendorName}");
                    }
                }
            }
        }
        else
        {
            ShowError("Invalid vendor ID.");
        }
    }
    
    private async Task GetWorkOrdersByClient(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter Client ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var clientId))
        {
            ShowInfo($"Fetching work orders for client {clientId}...");
            
            var response = await workOrderService.GetWorkOrdersByClientAsync(clientId);
            
            ShowSuccess($"Found {response.TotalCount} work orders for client {clientId}:");
            
            if (response.Data != null)
            {
                foreach (var workOrder in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                    if (!string.IsNullOrWhiteSpace(workOrder.ClientName))
                    {
                        System.Console.WriteLine($"    Client: {workOrder.ClientName}");
                    }
                }
            }
        }
        else
        {
            ShowError("Invalid client ID.");
        }
    }
    
    private async Task GetWorkOrdersByTechnician(IWorkOrderService workOrderService)
    {
        System.Console.Write("Enter Technician ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var technicianId))
        {
            ShowInfo($"Fetching work orders for technician {technicianId}...");
            
            var response = await workOrderService.GetWorkOrdersByTechnicianAsync(technicianId);
            
            ShowSuccess($"Found {response.TotalCount} work orders for technician {technicianId}:");
            
            if (response.Data != null)
            {
                foreach (var workOrder in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                    if (!string.IsNullOrWhiteSpace(workOrder.TechnicianName))
                    {
                        System.Console.WriteLine($"    Technician: {workOrder.TechnicianName}");
                    }
                    if (workOrder.NextVisit.HasValue)
                    {
                        System.Console.WriteLine($"    Next Visit: {workOrder.NextVisit:yyyy-MM-dd}");
                    }
                }
            }
        }
        else
        {
            ShowError("Invalid technician ID.");
        }
    }
    
    private async Task GetWorkOrdersByDateRange(IWorkOrderService workOrderService)
    {
        System.Console.WriteLine("\nTip: Use the same date for both start and end to search a single day");
        System.Console.Write("Enter start date (yyyy-MM-dd): ");
        var startDateStr = System.Console.ReadLine();
        System.Console.Write("Enter end date (yyyy-MM-dd): ");
        var endDateStr = System.Console.ReadLine();
        
        if (DateTime.TryParse(startDateStr, out var startDate) && DateTime.TryParse(endDateStr, out var endDate))
        {
            // Adjust end date to include the entire day
            endDate = endDate.AddDays(1).AddSeconds(-1);
            
            ShowInfo($"Fetching work orders between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}...");
            
            var response = await workOrderService.GetWorkOrdersByDateRangeAsync(startDate, endDate);
            
            ShowSuccess($"Found {response.TotalCount} work orders in date range:");
            
            if (response.Data != null)
            {
                foreach (var workOrder in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - WO #{workOrder.Id} - Status: {workOrder.Status}");
                    if (workOrder.NextVisit.HasValue)
                    {
                        System.Console.WriteLine($"    Next Visit: {workOrder.NextVisit:yyyy-MM-dd}");
                    }
                }
            }
        }
        else
        {
            ShowError("Invalid date format. Please use yyyy-MM-dd format.");
        }
    }
    
    private async Task TestVisitService()
    {
        System.Console.Clear();
        ShowHeader("Test Visit Service");
        
        // Create a scope to resolve scoped services
        using var scope = _services.CreateScope();
        var visitService = scope.ServiceProvider.GetRequiredService<IVisitService>();
        
        System.Console.WriteLine("\n=== Visit Service Menu ===");
        System.Console.WriteLine("1. Get all visits (paginated)");
        System.Console.WriteLine("2. Get visits by work order ID");
        System.Console.WriteLine("3. Get visits by technician ID");
        System.Console.WriteLine("4. Get visits by status");
        System.Console.WriteLine("5. Get visits by date range (use same date for single day)");
        System.Console.WriteLine("0. Back to main menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
        
        var choice = System.Console.ReadLine()?.Trim();
        
        try
        {
            switch (choice)
            {
                case "1":
                    await GetAllVisits(visitService);
                    break;
                case "2":
                    await GetVisitsByWorkOrder(visitService);
                    break;
                case "3":
                    await GetVisitsByTechnician(visitService);
                    break;
                case "4":
                    await GetVisitsByStatus(visitService);
                    break;
                case "5":
                    await GetVisitsByDateRange(visitService);
                    break;
                case "0":
                    return;
                default:
                    ShowError("Invalid choice.");
                    break;
            }
        }
        catch (FexaApiException ex)
        {
            ShowError($"API Error: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ResponseContent))
            {
                ShowError($"Response: {ex.ResponseContent}");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
    
    private async Task GetAllVisits(IVisitService visitService)
    {
        ShowInfo("Fetching visits (first 10)...");
        
        var parameters = new QueryParameters
        {
            Start = 0,
            Limit = 10
        };
        
        var response = await visitService.GetVisitsAsync(parameters);
        
        ShowSuccess($"Found {response.TotalCount} total visits. Showing first {response.Data?.Count() ?? 0}:");
        
        if (response.Data != null)
        {
            foreach (var visit in response.Data)
            {
                System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Status: {visit.Status} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd}");
            }
        }
    }
    
    private async Task GetVisitsByWorkOrder(IVisitService visitService)
    {
        System.Console.Write("Enter Work Order ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            ShowInfo($"Fetching visits for work order {workOrderId}...");
            
            var response = await visitService.GetVisitsByWorkOrderAsync(workOrderId);
            
            ShowSuccess($"Found {response.TotalCount} visits for work order {workOrderId}:");
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Status: {visit.Status}");
                    System.Console.WriteLine($"    Scheduled: {visit.StartDate:yyyy-MM-dd HH:mm} to {visit.EndDate:yyyy-MM-dd HH:mm}");
                    if (visit.CheckInTime.HasValue || visit.CheckOutTime.HasValue)
                    {
                        System.Console.WriteLine($"    Check-in: {visit.CheckInTime:yyyy-MM-dd HH:mm} | Check-out: {visit.CheckOutTime:yyyy-MM-dd HH:mm}");
                    }
                    if (!string.IsNullOrWhiteSpace(visit.StoreName))
                    {
                        System.Console.WriteLine($"    Store: {visit.StoreName}");
                    }
                }
            }
        }
        else
        {
            ShowError("Invalid work order ID.");
        }
    }
    
    private async Task GetVisitsByTechnician(IVisitService visitService)
    {
        System.Console.Write("Enter Technician ID: ");
        if (int.TryParse(System.Console.ReadLine(), out var technicianId))
        {
            ShowInfo($"Fetching visits for technician {technicianId}...");
            
            var response = await visitService.GetVisitsByTechnicianAsync(technicianId);
            
            ShowSuccess($"Found {response.TotalCount} visits for technician {technicianId}:");
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd}");
                }
            }
        }
        else
        {
            ShowError("Invalid technician ID.");
        }
    }
    
    private async Task GetVisitsByStatus(IVisitService visitService)
    {
        System.Console.Write("Enter status (e.g., scheduled, completed, cancelled): ");
        var status = System.Console.ReadLine()?.Trim();
        
        if (!string.IsNullOrEmpty(status))
        {
            ShowInfo($"Fetching visits with status '{status}'...");
            
            var response = await visitService.GetVisitsByStatusAsync(status);
            
            ShowSuccess($"Found {response.TotalCount} visits with status '{status}':");
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd}");
                }
            }
        }
        else
        {
            ShowError("Status cannot be empty.");
        }
    }
    
    private async Task GetVisitsByDateRange(IVisitService visitService)
    {
        System.Console.WriteLine("\nTip: Use the same date for both start and end to search a single day");
        System.Console.Write("Enter start date (yyyy-MM-dd): ");
        var startDateStr = System.Console.ReadLine();
        System.Console.Write("Enter end date (yyyy-MM-dd): ");
        var endDateStr = System.Console.ReadLine();
        
        if (DateTime.TryParse(startDateStr, out var startDate) && DateTime.TryParse(endDateStr, out var endDate))
        {
            ShowInfo($"Fetching visits between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}...");
            
            var response = await visitService.GetVisitsByDateRangeAsync(startDate, endDate);
            
            ShowSuccess($"Found {response.TotalCount} visits in date range:");
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd}");
                }
            }
        }
        else
        {
            ShowError("Invalid date format. Please use yyyy-MM-dd format.");
        }
    }
    
    private async Task GetVisitsBySpecificDate(IVisitService visitService)
    {
        System.Console.WriteLine("\nSelect date type:");
        System.Console.WriteLine("1. Start date (scheduled start time)");
        System.Console.WriteLine("2. Check-in date (actual visit time)");
        System.Console.Write("Enter choice (1-2): ");
        var dateType = System.Console.ReadLine()?.Trim();
        
        System.Console.Write("Enter date (yyyy-MM-dd): ");
        var dateStr = System.Console.ReadLine();
        
        if (DateTime.TryParse(dateStr, out var date))
        {
            PagedResponse<Visit> response;
            
            switch (dateType)
            {
                case "1":
                    ShowInfo($"Fetching visits with start date on {date:yyyy-MM-dd}...");
                    response = await visitService.GetVisitsByScheduledDateAsync(date);
                    ShowSuccess($"Found {response.TotalCount} visits starting on {date:yyyy-MM-dd}:");
                    break;
                case "2":
                    ShowInfo($"Fetching visits with check-in on {date:yyyy-MM-dd}...");
                    response = await visitService.GetVisitsByActualDateAsync(date);
                    ShowSuccess($"Found {response.TotalCount} visits checked in on {date:yyyy-MM-dd}:");
                    break;
                default:
                    ShowError("Invalid date type selection.");
                    return;
            }
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Status: {visit.Status} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd} - Actual: {visit.ActualDate:yyyy-MM-dd}");
                }
            }
        }
        else
        {
            ShowError("Invalid date format. Please use yyyy-MM-dd format.");
        }
    }
    
    private async Task GetVisitsBeforeAfterDate(IVisitService visitService)
    {
        System.Console.WriteLine("\nSelect search type:");
        System.Console.WriteLine("1. Visits starting after date");
        System.Console.WriteLine("2. Visits starting before date");
        System.Console.Write("Enter choice (1-2): ");
        var searchType = System.Console.ReadLine()?.Trim();
        
        System.Console.Write("Enter date (yyyy-MM-dd): ");
        var dateStr = System.Console.ReadLine();
        
        if (DateTime.TryParse(dateStr, out var date))
        {
            PagedResponse<Visit> response;
            
            switch (searchType)
            {
                case "1":
                    ShowInfo($"Fetching visits scheduled after {date:yyyy-MM-dd}...");
                    response = await visitService.GetVisitsScheduledAfterAsync(date);
                    ShowSuccess($"Found {response.TotalCount} visits scheduled after {date:yyyy-MM-dd}:");
                    break;
                case "2":
                    ShowInfo($"Fetching visits scheduled before {date:yyyy-MM-dd}...");
                    response = await visitService.GetVisitsScheduledBeforeAsync(date);
                    ShowSuccess($"Found {response.TotalCount} visits scheduled before {date:yyyy-MM-dd}:");
                    break;
                default:
                    ShowError("Invalid search type selection.");
                    return;
            }
            
            if (response.Data != null)
            {
                foreach (var visit in response.Data.Take(10))
                {
                    System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - Scheduled: {visit.ScheduledDate:yyyy-MM-dd} - Status: {visit.Status}");
                }
            }
        }
        else
        {
            ShowError("Invalid date format. Please use yyyy-MM-dd format.");
        }
    }
    
    private async Task GetVisitsWithCustomFilters(IVisitService visitService)
    {
        ShowInfo("Building custom filter for visits...");
        ShowInfo("Example: Visits for work orders 116 and 117 with status 'completed'");
        
        var parameters = QueryParameters.Create()
            .WithFilters(filters => filters
                .WhereWorkOrderIds(116, 117)
                .WhereVisitStatus("completed"));
        
        ShowInfo("Executing query with custom filters...");
        
        var response = await visitService.GetVisitsAsync(parameters);
        
        ShowSuccess($"Found {response.TotalCount} visits matching filters:");
        
        if (response.Data != null)
        {
            foreach (var visit in response.Data.Take(10))
            {
                System.Console.WriteLine($"  - Visit #{visit.Id}: {visit.VisitNumber} - WO: {visit.WorkOrderId} - Status: {visit.Status}");
            }
        }
    }
    
    private void ShowHeader(string title)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"\n>>> {title} <<<");
        System.Console.ResetColor();
        System.Console.WriteLine(new string('-', title.Length + 8));
    }
    
    private void ShowInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"ℹ️  {message}");
        System.Console.ResetColor();
    }
    
    private void ShowSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"✅ {message}");
        System.Console.ResetColor();
    }
    
    private void ShowError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"❌ {message}");
        System.Console.ResetColor();
    }
    
    private void ShowWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"⚠️  {message}");
        System.Console.ResetColor();
    }
    
    private void ShowPressAnyKey()
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine("Press any key to continue...");
        System.Console.ResetColor();
        System.Console.ReadKey(true);
    }
    
    private void ShowGoodbye()
    {
        System.Console.Clear();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("\nThank you for using Fexa API Client Test Console!");
        System.Console.WriteLine("Goodbye! 👋");
        System.Console.ResetColor();
    }
    
    private async Task TestTransitionService()
    {
        System.Console.Clear();
        ShowHeader("Workflow Statuses & Transitions");
        
        using var scope = _services.CreateScope();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        
        System.Console.WriteLine("\n=== Transition Service Menu ===");
        System.Console.WriteLine("1. Get all transitions");
        System.Console.WriteLine("2. Get unique statuses");
        System.Console.WriteLine("3. Get transitions by type (Assignment/Client Invoice)");
        System.Console.WriteLine("4. Get transitions from a specific status");
        System.Console.WriteLine("5. Get transitions to a specific status");
        System.Console.WriteLine("6. Show Work Order specific statuses");
        System.Console.WriteLine("0. Back to main menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
        
        var choice = System.Console.ReadLine()?.Trim();
        
        try
        {
            switch (choice)
            {
                case "1":
                    await GetAllTransitions(transitionService);
                    break;
                case "2":
                    await GetUniqueStatuses(transitionService);
                    break;
                case "3":
                    await GetTransitionsByType(transitionService);
                    break;
                case "4":
                    await GetTransitionsFromStatus(transitionService);
                    break;
                case "5":
                    await GetTransitionsToStatus(transitionService);
                    break;
                case "6":
                    await ShowWorkOrderStatuses(transitionService);
                    break;
                case "0":
                    return;
                default:
                    ShowError("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
    
    private async Task GetAllTransitions(ITransitionService transitionService)
    {
        ShowInfo("Fetching ALL workflow transitions (this may take a moment)...");
        
        var allTransitions = await transitionService.GetAllTransitionsAsync();
        
        if (allTransitions.Any())
        {
            ShowSuccess($"Found {allTransitions.Count} transitions total");
            
            // Group by workflow object type
            var byType = allTransitions.GroupBy(t => t.WorkflowObjectType);
            
            foreach (var group in byType)
            {
                System.Console.WriteLine($"\n{group.Key} ({group.Count()} transitions):");
                
                // Show first 5 transitions for each type
                foreach (var transition in group.Take(5))
                {
                    System.Console.WriteLine($"  - {transition.Name}:");
                    System.Console.WriteLine($"    From: {transition.FromStatus?.Name} (ID: {transition.FromStatusId})");
                    System.Console.WriteLine($"    To: {transition.ToStatus?.Name} (ID: {transition.ToStatusId})");
                }
                
                if (group.Count() > 5)
                {
                    System.Console.WriteLine($"  ... and {group.Count() - 5} more");
                }
            }
            
            // Show option to display a single page
            System.Console.Write("\nFetch a specific page? (y/n): ");
            if (System.Console.ReadLine()?.ToLower() == "y")
            {
                System.Console.Write("Enter start index (default 0): ");
                var startStr = System.Console.ReadLine();
                var start = string.IsNullOrWhiteSpace(startStr) ? 0 : int.Parse(startStr);
                
                System.Console.Write("Enter limit (default 100): ");
                var limitStr = System.Console.ReadLine();
                var limit = string.IsNullOrWhiteSpace(limitStr) ? 100 : int.Parse(limitStr);
                
                var pageResponse = await transitionService.GetTransitionsAsync(start, limit);
                ShowInfo($"Page contains {pageResponse.Transitions?.Count ?? 0} transitions (total available: {pageResponse.TotalCount})");
            }
        }
        else
        {
            ShowError("No transitions found");
        }
    }
    
    private async Task GetUniqueStatuses(ITransitionService transitionService)
    {
        ShowInfo("Fetching all workflow transitions to identify object types...");
        
        // Get all transitions from all pages
        var allTransitions = await transitionService.GetAllTransitionsAsync();
        
        if (!allTransitions.Any())
        {
            ShowError("No transitions found");
            return;
        }
        
        // Get unique workflow object types
        var workflowTypes = allTransitions
            .Select(t => t.WorkflowObjectType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        
        ShowSuccess($"Found {workflowTypes.Count} workflow object types from {allTransitions.Count} transitions");
        
        // Display available workflow types
        System.Console.WriteLine("\n=== Available Workflow Object Types ===");
        for (int i = 0; i < workflowTypes.Count; i++)
        {
            var typeTransitions = allTransitions.Count(t => t.WorkflowObjectType == workflowTypes[i]);
            System.Console.WriteLine($"{i + 1}. {workflowTypes[i]} ({typeTransitions} transitions)");
        }
        System.Console.WriteLine("0. Show all types");
        
        System.Console.Write("\nSelect workflow type to view statuses (enter number): ");
        var choice = System.Console.ReadLine()?.Trim();
        
        if (!int.TryParse(choice, out var selection))
        {
            ShowError("Invalid selection");
            return;
        }
        
        if (selection == 0)
        {
            // Show statuses for all types
            foreach (var workflowType in workflowTypes)
            {
                ShowStatusesForWorkflowType(workflowType, allTransitions);
            }
        }
        else if (selection > 0 && selection <= workflowTypes.Count)
        {
            // Show statuses for selected type
            var selectedType = workflowTypes[selection - 1];
            ShowStatusesForWorkflowType(selectedType, allTransitions);
        }
        else
        {
            ShowError("Invalid selection");
        }
    }
    
    private void ShowStatusesForWorkflowType(string workflowType, List<WorkflowTransition> allTransitions)
    {
        // Filter transitions for this workflow type
        var typeTransitions = allTransitions.Where(t => t.WorkflowObjectType == workflowType).ToList();
        
        // Extract unique statuses from these transitions
        var uniqueStatuses = new HashSet<WorkflowStatus>(new WorkflowStatusComparer());
        
        foreach (var transition in typeTransitions)
        {
            if (transition.FromStatus != null)
            {
                uniqueStatuses.Add(transition.FromStatus);
            }
            
            if (transition.ToStatus != null)
            {
                uniqueStatuses.Add(transition.ToStatus);
            }
        }
        
        // Sort statuses by ID for display
        var sortedStatuses = uniqueStatuses.OrderBy(s => s.Id).ToList();
        
        System.Console.WriteLine($"\n=== {workflowType} Statuses ({sortedStatuses.Count} unique) ===");
        System.Console.WriteLine("ID    | Status Name");
        System.Console.WriteLine("------|--------------------------------------------------");
        foreach (var status in sortedStatuses)
        {
            System.Console.WriteLine($"{status.Id,-5} | {status.Name}");
        }
        
        // Group by category for better organization
        System.Console.WriteLine($"\n=== {workflowType} Statuses Grouped by Category ===");
        ShowStatusesByCategory(sortedStatuses);
    }
    
    // Helper class for comparing WorkflowStatus objects by ID
    private class WorkflowStatusComparer : IEqualityComparer<WorkflowStatus>
    {
        public bool Equals(WorkflowStatus? x, WorkflowStatus? y)
        {
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }
        
        public int GetHashCode(WorkflowStatus obj)
        {
            return obj.Id.GetHashCode();
        }
    }
    
    private void ShowStatusesByCategory(List<WorkflowStatus> statuses)
    {
        // Group by category
        var grouped = statuses.GroupBy(s => 
        {
            if (s.Name.StartsWith("Completed")) return "Completed";
            if (s.Name.StartsWith("Cancelled")) return "Cancelled";
            if (s.Name.StartsWith("In Progress")) return "In Progress";
            if (s.Name.Contains("Pending")) return "Pending";
            if (s.Name.Contains("Approved")) return "Approved";
            if (s.Name.Contains("Scheduled")) return "Scheduled";
            if (s.Name.Contains("Accepted")) return "Accepted";
            if (s.Name.Contains("Declined")) return "Declined";
            if (s.Name.StartsWith("Needs") || s.Name.Contains("Needed")) return "Needs Action";
            return "Other";
        });
        
        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            System.Console.WriteLine($"\n  {group.Key}:");
            foreach (var status in group.OrderBy(s => s.Name))
            {
                System.Console.WriteLine($"    - {status.Name} (ID: {status.Id})");
            }
        }
    }
    
    private async Task GetTransitionsByType(ITransitionService transitionService)
    {
        ShowInfo("Fetching all transitions and grouping by type...");
        
        // Get all transitions once
        var allTransitions = await transitionService.GetAllTransitionsAsync();
        
        if (!allTransitions.Any())
        {
            ShowError("No transitions found");
            return;
        }
        
        // Group by workflow object type
        var byType = allTransitions.GroupBy(t => t.WorkflowObjectType)
            .OrderBy(g => g.Key)
            .ToList();
        
        ShowSuccess($"Found {allTransitions.Count} total transitions across {byType.Count} workflow types");
        
        // Show menu of available types
        System.Console.WriteLine("\n=== Select Workflow Type ===");
        for (int i = 0; i < byType.Count; i++)
        {
            System.Console.WriteLine($"{i + 1}. {byType[i].Key} ({byType[i].Count()} transitions)");
        }
        System.Console.WriteLine("0. Show all types");
        System.Console.Write("\nEnter your choice: ");
        
        var choice = System.Console.ReadLine()?.Trim();
        
        if (choice == "0")
        {
            // Show all types
            foreach (var typeGroup in byType)
            {
                ShowTransitionsForType(typeGroup.Key, typeGroup.ToList());
            }
        }
        else if (int.TryParse(choice, out var index) && index > 0 && index <= byType.Count)
        {
            var selectedType = byType[index - 1];
            ShowTransitionsForType(selectedType.Key, selectedType.ToList());
        }
        else
        {
            ShowError("Invalid choice");
        }
    }
    
    private void ShowTransitionsForType(string typeName, List<WorkflowTransition> transitions)
    {
        System.Console.WriteLine($"\n=== {typeName} Transitions ({transitions.Count} total) ===");
        
        // Group by from status for better readability
        var byFromStatus = transitions.GroupBy(t => t.FromStatus?.Name ?? "Unknown")
            .OrderBy(g => g.Key)
            .ToList();
        
        // Show first 10 status groups
        foreach (var group in byFromStatus.Take(10))
        {
            System.Console.WriteLine($"\nFrom '{group.Key}':");
            foreach (var transition in group)
            {
                System.Console.WriteLine($"  → {transition.Name} → '{transition.ToStatus?.Name}'");
            }
        }
        
        if (byFromStatus.Count > 10)
        {
            System.Console.WriteLine($"\n... and {byFromStatus.Count - 10} more status groups");
            
            System.Console.Write("\nShow all status groups? (y/n): ");
            if (System.Console.ReadLine()?.ToLower() == "y")
            {
                foreach (var group in byFromStatus.Skip(10))
                {
                    System.Console.WriteLine($"\nFrom '{group.Key}':");
                    foreach (var transition in group)
                    {
                        System.Console.WriteLine($"  → {transition.Name} → '{transition.ToStatus?.Name}'");
                    }
                }
            }
        }
    }
    
    private async Task GetTransitionsFromStatus(ITransitionService transitionService)
    {
        System.Console.Write("Enter status ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var statusId))
        {
            ShowError("Invalid status ID");
            return;
        }
        
        ShowInfo($"Fetching transitions from status ID {statusId}...");
        
        var transitions = await transitionService.GetTransitionsFromStatusAsync(statusId);
        
        if (transitions.Any())
        {
            var statusName = transitions.First().FromStatus?.Name ?? "Unknown";
            ShowSuccess($"Found {transitions.Count} transitions from '{statusName}' (ID: {statusId})");
            
            foreach (var transition in transitions)
            {
                System.Console.WriteLine($"  - {transition.Name} → '{transition.ToStatus?.Name}' (ID: {transition.ToStatusId})");
            }
        }
        else
        {
            ShowInfo($"No transitions found from status ID {statusId}");
        }
    }
    
    private async Task GetTransitionsToStatus(ITransitionService transitionService)
    {
        System.Console.Write("Enter status ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var statusId))
        {
            ShowError("Invalid status ID");
            return;
        }
        
        ShowInfo($"Fetching transitions to status ID {statusId}...");
        
        var transitions = await transitionService.GetTransitionsToStatusAsync(statusId);
        
        if (transitions.Any())
        {
            var statusName = transitions.First().ToStatus?.Name ?? "Unknown";
            ShowSuccess($"Found {transitions.Count} transitions to '{statusName}' (ID: {statusId})");
            
            foreach (var transition in transitions)
            {
                System.Console.WriteLine($"  - '{transition.FromStatus?.Name}' (ID: {transition.FromStatusId}) → {transition.Name}");
            }
        }
        else
        {
            ShowInfo($"No transitions found to status ID {statusId}");
        }
    }
    
    private async Task ShowWorkOrderStatuses(ITransitionService transitionService)
    {
        ShowInfo("Fetching Work Order specific statuses...");
        
        // Use the dedicated method that leverages the cache
        var workOrderStatuses = await transitionService.GetWorkOrderStatusesAsync();
        
        ShowSuccess($"Found {workOrderStatuses.Count} unique statuses for Work Orders");
        
        System.Console.WriteLine("\nCommonly used Work Order statuses:");
        var commonStatuses = new[] { "New", "Action Required", "In Progress", "Completed", "Cancelled", "Scheduled", "Pending" };
        
        var foundCommon = workOrderStatuses
            .Where(s => commonStatuses.Any(cs => s.Name.Contains(cs, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(s => s.Name)
            .ToList();
            
        foreach (var status in foundCommon)
        {
            System.Console.WriteLine($"  - {status.Name} (ID: {status.Id})");
        }
        
        System.Console.WriteLine("\nAll Work Order statuses:");
        foreach (var status in workOrderStatuses)
        {
            System.Console.WriteLine($"  - {status.Name} (ID: {status.Id})");
        }
        
        // Also get transitions to show workflow
        System.Console.Write("\nShow workflow transitions for a specific status? (y/n): ");
        if (System.Console.ReadLine()?.ToLower() == "y")
        {
            System.Console.Write("Enter status ID: ");
            if (int.TryParse(System.Console.ReadLine(), out var statusId))
            {
                var transitionsFrom = await transitionService.GetTransitionsFromStatusAsync(statusId);
                var transitionsTo = await transitionService.GetTransitionsToStatusAsync(statusId);
                
                var workOrderTransitionsFrom = transitionsFrom.Where(t => t.WorkflowObjectType == "Work Order").ToList();
                var workOrderTransitionsTo = transitionsTo.Where(t => t.WorkflowObjectType == "Work Order").ToList();
                
                if (workOrderTransitionsFrom.Any())
                {
                    System.Console.WriteLine($"\nWork Order transitions FROM this status ({workOrderTransitionsFrom.Count}):");
                    foreach (var t in workOrderTransitionsFrom)
                    {
                        System.Console.WriteLine($"  - {t.Name}: {t.FromStatus?.Name} → {t.ToStatus?.Name}");
                    }
                }
                
                if (workOrderTransitionsTo.Any())
                {
                    System.Console.WriteLine($"\nWork Order transitions TO this status ({workOrderTransitionsTo.Count}):");
                    foreach (var t in workOrderTransitionsTo)
                    {
                        System.Console.WriteLine($"  - {t.Name}: {t.FromStatus?.Name} → {t.ToStatus?.Name}");
                    }
                }
            }
        }
    }
    
    private async Task UpdateWorkOrderStatus()
    {
        using var scope = _services.CreateScope();
        var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();
        var transitionService = scope.ServiceProvider.GetRequiredService<ITransitionService>();
        
        ShowInfo("=== Update Work Order Status ===");
        
        // Step 1: Get work order number
        System.Console.Write("Enter Work Order Number: ");
        var input = System.Console.ReadLine()?.Trim();
        
        if (!int.TryParse(input, out var workOrderId))
        {
            ShowError("Invalid work order number.");
            return;
        }
        
        try
        {
            // Step 2: Look up work order and display info
            ShowInfo($"Fetching work order {workOrderId}...");
            var workOrder = await workOrderService.GetWorkOrderAsync(workOrderId);
            
            System.Console.WriteLine("\n=== Work Order Details ===");
            System.Console.WriteLine($"ID: {workOrder.Id}");
            System.Console.WriteLine($"Description: {workOrder.Description}");
            System.Console.WriteLine($"Status: {workOrder.ObjectState?.Status?.Name ?? "Unknown"}");
            System.Console.WriteLine($"Status ID: {workOrder.ObjectState?.StatusId ?? 0}");
            System.Console.WriteLine($"Workflow Type ID: {workOrder.ObjectState?.Status?.WorkflowType?.Id}");
            System.Console.WriteLine($"Workflow Type Name: {workOrder.ObjectState?.Status?.WorkflowType?.Name}");
            System.Console.WriteLine($"Priority ID: {workOrder.PriorityId ?? 0}");
            System.Console.WriteLine($"Store: {workOrder.StoreName} ({workOrder.StoreCity}, {workOrder.StoreState})");
            System.Console.WriteLine($"Created: {workOrder.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            System.Console.WriteLine($"Updated: {workOrder.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            
            var currentStatusId = workOrder.ObjectState?.StatusId ?? 0;
            var currentStatusName = workOrder.ObjectState?.Status?.Name ?? "Unknown";
            
            if (currentStatusId == 0)
            {
                ShowError("Could not determine current status of work order.");
                return;
            }
            
            // Step 3: Get all transitions to find valid next statuses
            ShowInfo("Fetching available status transitions...");
            var allTransitions = await transitionService.GetAllTransitionsAsync();
            
            // Debug: Let's see all unique workflow object types in transitions
            var workflowTypes = allTransitions
                .Select(t => t.WorkflowObjectType)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            
            System.Console.WriteLine($"\nAvailable workflow object types in transitions:");
            foreach (var type in workflowTypes)
            {
                System.Console.WriteLine($"  - {type}");
            }
            
            // Filter for Work Order transitions only
            var transitionsFromStatus = allTransitions
                .Where(t => t.WorkflowObjectType == "Work Order" && t.FromStatusId == currentStatusId)
                .ToList();
            
            if (!transitionsFromStatus.Any())
            {
                ShowWarning($"No transitions found from current status '{currentStatusName}' (ID: {currentStatusId}).");
                
                // Work orders might not use transitions - let's allow manual status selection
                System.Console.Write("\nWould you like to see all available work order statuses? (y/n): ");
                var showAll = System.Console.ReadLine()?.ToLower();
                
                if (showAll != "y" && showAll != "yes")
                {
                    return;
                }
                
                // Get all unique statuses from transitions to show available options
                var allStatuses = allTransitions
                    .SelectMany(t => new[] { t.FromStatus, t.ToStatus })
                    .Where(s => s != null)
                    .Select(s => new { s!.Id, s.Name })
                    .Distinct()
                    .OrderBy(s => s.Name)
                    .ToList();
                
                System.Console.WriteLine($"\n=== All Available Statuses ({allStatuses.Count}) ===");
                for (int i = 0; i < allStatuses.Count; i++)
                {
                    System.Console.WriteLine($"{i + 1}. {allStatuses[i].Name} (ID: {allStatuses[i].Id})");
                }
                
                System.Console.WriteLine("0. Cancel");
                System.Console.Write("\nSelect target status: ");
                
                var manualChoice = System.Console.ReadLine()?.Trim();
                if (!int.TryParse(manualChoice, out var manualIndex) || manualIndex < 0 || manualIndex > allStatuses.Count)
                {
                    ShowError("Invalid selection.");
                    return;
                }
                
                if (manualIndex == 0)
                {
                    ShowInfo("Operation cancelled.");
                    return;
                }
                
                var manualSelectedStatus = allStatuses[manualIndex - 1];
                
                // Confirm and update without transition validation
                System.Console.Write($"\nConfirm status update from '{currentStatusName}' to '{manualSelectedStatus.Name}'? (y/n): ");
                var manualConfirm = System.Console.ReadLine()?.ToLower();
                
                if (manualConfirm != "y" && manualConfirm != "yes")
                {
                    ShowInfo("Operation cancelled.");
                    return;
                }
                
                ShowInfo($"Updating work order status to '{manualSelectedStatus.Name}'...\n");
                
                try
                {
                    var manualUpdatedWorkOrder = await workOrderService.UpdateStatusAsync(
                        workOrderId, 
                        manualSelectedStatus.Id, 
                        null  // API doesn't support reason in body
                    );
                    
                    ShowSuccess($"Successfully updated work order {workOrderId} status!");
                    System.Console.WriteLine($"New Status: {manualUpdatedWorkOrder.ObjectState?.Status?.Name ?? "Unknown"}");
                    System.Console.WriteLine($"Status ID: {manualUpdatedWorkOrder.ObjectState?.StatusId ?? 0}");
                }
                catch (Exception ex)
                {
                    ShowError($"Error updating status: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ShowError($"Inner exception: {ex.InnerException.Message}");
                    }
                }
                
                return;
            }
            
            // Group transitions by workflow type
            var transitionsByType = transitionsFromStatus
                .GroupBy(t => t.WorkflowObjectType)
                .ToList();
            
            System.Console.WriteLine($"\nFound {transitionsFromStatus.Count} transitions from '{currentStatusName}':");
            foreach (var group in transitionsByType)
            {
                System.Console.WriteLine($"  - {group.Key}: {group.Count()} transitions");
            }
            
            // For now, use all transitions regardless of type
            // Work orders might not use "Assignment" type - they might have their own type
            var validTransitions = transitionsFromStatus;
            
            // Get unique target statuses
            var targetStatuses = validTransitions
                .Where(t => t.ToStatus != null)
                .Select(t => new { t.ToStatusId, Name = t.ToStatus!.Name })
                .Distinct()
                .OrderBy(s => s.Name)
                .ToList();
            
            System.Console.WriteLine($"\n=== Available Status Transitions ===");
            System.Console.WriteLine($"Current Status: {currentStatusName} (ID: {currentStatusId})");
            
            ShowWarning("Note: Some transitions may have additional requirements (e.g., completed fields, assignments in specific states)");
            ShowWarning("that must be met before they can be used, even if they appear in this list.");
            
            System.Console.WriteLine("\nYou can transition to:");
            
            for (int i = 0; i < targetStatuses.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {targetStatuses[i].Name} (ID: {targetStatuses[i].ToStatusId})");
            }
            
            System.Console.WriteLine("0. Cancel");
            System.Console.Write("\nSelect target status: ");
            
            var statusChoice = System.Console.ReadLine()?.Trim();
            if (!int.TryParse(statusChoice, out var choiceIndex) || choiceIndex < 0 || choiceIndex > targetStatuses.Count)
            {
                ShowError("Invalid selection.");
                return;
            }
            
            if (choiceIndex == 0)
            {
                ShowInfo("Operation cancelled.");
                return;
            }
            
            var selectedStatus = targetStatuses[choiceIndex - 1];
            
            // Step 4: Validate the specific transition
            var transition = validTransitions.FirstOrDefault(t => t.ToStatusId == selectedStatus.ToStatusId);
            if (transition == null)
            {
                ShowError($"Invalid transition from '{currentStatusName}' to '{selectedStatus.Name}'.");
                return;
            }
            
            System.Console.WriteLine($"\nTransition Details:");
            System.Console.WriteLine($"  Name: {transition.Name}");
            System.Console.WriteLine($"  From: {transition.FromStatus?.Name} (ID: {transition.FromStatusId})");
            System.Console.WriteLine($"  To: {transition.ToStatus?.Name} (ID: {transition.ToStatusId})");
            
            // Step 5: Confirm the update
            System.Console.Write($"\nConfirm status update from '{currentStatusName}' to '{selectedStatus.Name}'? (y/n): ");
            var confirm = System.Console.ReadLine()?.ToLower();
            
            if (confirm != "y" && confirm != "yes")
            {
                ShowInfo("Operation cancelled.");
                return;
            }
            
            // Step 7: Update the status
            ShowInfo($"Updating work order status to '{selectedStatus.Name}'...");
            
            try
            {
                var updatedWorkOrder = await workOrderService.UpdateStatusAsync(
                    workOrderId, 
                    selectedStatus.ToStatusId, 
                    null  // API doesn't support reason in body
                );
                
                // Double-check that the status actually changed
                var newStatusId = updatedWorkOrder.ObjectState?.StatusId ?? 0;
                var newStatusName = updatedWorkOrder.ObjectState?.Status?.Name ?? "Unknown";
                
                if (newStatusId == selectedStatus.ToStatusId)
                {
                    ShowSuccess($"Successfully updated work order {workOrderId} status!");
                    System.Console.WriteLine($"New Status: {newStatusName}");
                    System.Console.WriteLine($"Status ID: {newStatusId}");
                }
                else
                {
                    ShowError($"Status update failed! Work order status is still '{newStatusName}' (ID: {newStatusId})");
                    ShowError($"Expected status was '{selectedStatus.Name}' (ID: {selectedStatus.ToStatusId})");
                }
            }
            catch (InvalidOperationException ex)
            {
                ShowError($"Failed to update status: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating status: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ShowError($"Details: {ex.InnerException.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error updating work order status: {ex.Message}");
            if (ex.InnerException != null)
            {
                ShowError($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
    
    private async Task TestClientService()
    {
        System.Console.Clear();
        ShowHeader("Test Client Service");
        
        var testClientService = new TestClientService(_services);
        await testClientService.RunTests();
    }
    
    private async Task TestVendorService()
    {
        System.Console.Clear();
        ShowHeader("Test Vendor Service");
        
        var testVendorService = new TestVendorService(_services);
        await testVendorService.RunTests();
    }
    
    private async Task TestWorkOrderCategoryService()
    {
        System.Console.Clear();
        ShowHeader("Test Work Order Category Service");
        
        // WorkOrderCategoryService is singleton, so no scope needed
        var categoryService = _services.GetRequiredService<IWorkOrderCategoryService>();
        
        System.Console.WriteLine("\n=== Category Service Menu ===");
        System.Console.WriteLine("1. Get all categories");
        System.Console.WriteLine("2. Get simplified categories (with hierarchy)");
        System.Console.WriteLine("3. Get active simplified categories only");
        System.Console.WriteLine("4. Get root categories");
        System.Console.WriteLine("5. Get category by path (e.g., 'Plumbing | Grease Trap')");
        System.Console.WriteLine("6. View sub-categories by parent (interactive)");
        System.Console.WriteLine("7. Get cache status");
        System.Console.WriteLine("8. Refresh cache synchronously");
        System.Console.WriteLine("9. Refresh cache asynchronously (background)");
        System.Console.WriteLine("0. Back to main menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
        
        var choice = System.Console.ReadLine()?.Trim();
        
        try
        {
            switch (choice)
            {
                case "1":
                    var allCategories = await categoryService.GetAllCategoriesAsync();
                    ShowSuccess($"Found {allCategories.Count} categories");
                    foreach (var cat in allCategories.Take(10))
                    {
                        System.Console.WriteLine($"  - {cat.Id}: {cat.Category} (Active: {cat.Active}, IsLeaf: {cat.IsLeaf})");
                        if (!string.IsNullOrEmpty(cat.CategoryWithAllAncestors))
                        {
                            System.Console.WriteLine($"    Full Path: {cat.CategoryWithAllAncestors}");
                        }
                    }
                    if (allCategories.Count > 10)
                    {
                        System.Console.WriteLine($"  ... and {allCategories.Count - 10} more");
                    }
                    break;
                    
                case "2":
                    var hierarchyResponse = await categoryService.GetSimplifiedCategoriesAsync();
                    ShowSuccess($"Found {hierarchyResponse.Categories.Count} simplified categories");
                    if (hierarchyResponse.Warnings?.Any() == true)
                    {
                        ShowWarning($"Hierarchy has {hierarchyResponse.Warnings.Count} warnings:");
                        foreach (var warning in hierarchyResponse.Warnings.Take(5))
                        {
                            System.Console.WriteLine($"  - {warning}");
                        }
                    }
                    foreach (var cat in hierarchyResponse.Categories.Take(10))
                    {
                        System.Console.WriteLine($"  - {cat.Id}: {cat.Category}");
                        System.Console.WriteLine($"    Full Path: {cat.FullPath}");
                        System.Console.WriteLine($"    Active: {cat.Active}, IsLeaf: {cat.IsLeaf}, ParentId: {cat.ParentId ?? 0}");
                    }
                    break;
                    
                case "3":
                    var activeCategories = await categoryService.GetActiveSimplifiedCategoriesAsync();
                    ShowSuccess($"Found {activeCategories.Count} active categories");
                    foreach (var cat in activeCategories.Take(10))
                    {
                        System.Console.WriteLine($"  - {cat.Id}: {cat.Category} - {cat.FullPath}");
                    }
                    break;
                    
                case "4":
                    var rootCategories = await categoryService.GetRootCategoriesAsync();
                    ShowSuccess($"Found {rootCategories.Count} root categories");
                    foreach (var cat in rootCategories)
                    {
                        System.Console.WriteLine($"  - {cat.Id}: {cat.Category}");
                    }
                    break;
                    
                case "5":
                    System.Console.Write("Enter category path (e.g., 'Plumbing | Grease Trap'): ");
                    var path = System.Console.ReadLine();
                    if (!string.IsNullOrEmpty(path))
                    {
                        var category = await categoryService.GetByFullPathAsync(path);
                        if (category != null)
                        {
                            ShowSuccess($"Found category:");
                            System.Console.WriteLine($"  ID: {category.Id}");
                            System.Console.WriteLine($"  Name: {category.Category}");
                            System.Console.WriteLine($"  Full Path: {category.FullPath}");
                            System.Console.WriteLine($"  Active: {category.Active}");
                            System.Console.WriteLine($"  IsLeaf: {category.IsLeaf}");
                            System.Console.WriteLine($"  ParentId: {category.ParentId ?? 0}");
                        }
                        else
                        {
                            ShowWarning($"Category not found for path: {path}");
                        }
                    }
                    break;
                    
                case "6":
                    await ViewSubCategoriesByParent(categoryService);
                    break;
                    
                case "7":
                    var status = await categoryService.GetCacheStatusAsync();
                    ShowInfo("Cache Status:");
                    System.Console.WriteLine($"  Last Refreshed: {status.LastRefreshed:yyyy-MM-dd HH:mm:ss}");
                    System.Console.WriteLine($"  Cache Age: {status.CacheAge.TotalMinutes:F1} minutes");
                    System.Console.WriteLine($"  Item Count: {status.ItemCount}");
                    System.Console.WriteLine($"  Is Refreshing: {status.IsRefreshing}");
                    if (status.LastRefreshAttempt.HasValue)
                    {
                        System.Console.WriteLine($"  Last Refresh Attempt: {status.LastRefreshAttempt:yyyy-MM-dd HH:mm:ss}");
                    }
                    System.Console.WriteLine($"  Last Refresh Successful: {status.LastRefreshSuccessful}");
                    break;
                    
                case "8":
                    ShowInfo("Refreshing cache synchronously (please wait)...");
                    var refreshedCategories = await categoryService.RefreshCacheAsync();
                    ShowSuccess($"Cache refreshed with {refreshedCategories.Count} categories");
                    break;
                    
                case "9":
                    ShowInfo("Starting background cache refresh...");
                    await categoryService.RefreshCacheInBackgroundAsync();
                    ShowSuccess("Background refresh started. Check cache status to monitor progress.");
                    break;
                    
                case "0":
                    return;
                    
                default:
                    ShowError("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
    
    private async Task ViewSubCategoriesByParent(IWorkOrderCategoryService categoryService)
    {
        try
        {
            ShowInfo("Loading category hierarchy...");
            
            // Get all categories to work with
            var hierarchyResponse = await categoryService.GetSimplifiedCategoriesAsync();
            var allCategories = hierarchyResponse.Categories;
            
            if (!allCategories.Any())
            {
                ShowWarning("No categories found. Please refresh the cache.");
                return;
            }
            
            // Start with root categories
            var currentParentId = (int?)null;
            var breadcrumb = new Stack<(int? id, string name)>();
            breadcrumb.Push((null, "Root"));
            
            while (true)
            {
                // Get categories at current level
                var categoriesAtLevel = allCategories
                    .Where(c => c.ParentId == currentParentId)
                    .OrderBy(c => c.Category)
                    .ToList();
                
                if (!categoriesAtLevel.Any())
                {
                    ShowWarning($"No sub-categories found at this level.");
                    
                    if (breadcrumb.Count > 1)
                    {
                        // Go back up one level
                        breadcrumb.Pop();
                        var parent = breadcrumb.Peek();
                        currentParentId = parent.id;
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                
                // Display current location in hierarchy
                System.Console.WriteLine();
                ShowHeader($"Category Navigation - Level: {string.Join(" > ", breadcrumb.Reverse().Select(b => b.name))}");
                
                // Show categories at this level
                System.Console.WriteLine($"\nCategories at this level ({categoriesAtLevel.Count} items):");
                System.Console.WriteLine("──────────────────────────────────────────────────");
                
                for (int i = 0; i < categoriesAtLevel.Count; i++)
                {
                    var cat = categoriesAtLevel[i];
                    var childCount = allCategories.Count(c => c.ParentId == cat.Id);
                    var leafIndicator = cat.IsLeaf ? " [LEAF]" : $" [{childCount} children]";
                    var activeIndicator = cat.Active ? "" : " [INACTIVE]";
                    
                    System.Console.WriteLine($"{i + 1,3}. {cat.Category}{leafIndicator}{activeIndicator}");
                    
                    // Show full path in gray
                    System.Console.ForegroundColor = ConsoleColor.DarkGray;
                    System.Console.WriteLine($"     Path: {cat.FullPath}");
                    System.Console.ResetColor();
                }
                
                // Show navigation options
                System.Console.WriteLine("──────────────────────────────────────────────────");
                System.Console.WriteLine("\nOptions:");
                System.Console.WriteLine("  Enter number to drill into that category");
                System.Console.WriteLine("  'b' to go back to parent level");
                System.Console.WriteLine("  'r' to return to root categories");
                System.Console.WriteLine("  'd' to show details of a category");
                System.Console.WriteLine("  'q' to quit navigation");
                System.Console.WriteLine();
                System.Console.Write("Your choice: ");
                
                var input = System.Console.ReadLine()?.Trim().ToLower();
                
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                
                if (input == "q")
                {
                    return;
                }
                else if (input == "b")
                {
                    if (breadcrumb.Count > 1)
                    {
                        breadcrumb.Pop();
                        var parent = breadcrumb.Peek();
                        currentParentId = parent.id;
                    }
                    else
                    {
                        ShowInfo("Already at root level");
                    }
                }
                else if (input == "r")
                {
                    // Return to root
                    breadcrumb.Clear();
                    breadcrumb.Push((null, "Root"));
                    currentParentId = null;
                }
                else if (input == "d")
                {
                    System.Console.Write("Enter category number to view details: ");
                    if (int.TryParse(System.Console.ReadLine(), out var detailIndex) && 
                        detailIndex > 0 && detailIndex <= categoriesAtLevel.Count)
                    {
                        var cat = categoriesAtLevel[detailIndex - 1];
                        System.Console.WriteLine();
                        ShowInfo($"Category Details:");
                        System.Console.WriteLine($"  ID: {cat.Id}");
                        System.Console.WriteLine($"  Name: {cat.Category}");
                        System.Console.WriteLine($"  Full Path: {cat.FullPath}");
                        System.Console.WriteLine($"  Description: {cat.Description ?? "(none)"}");
                        System.Console.WriteLine($"  Active: {cat.Active}");
                        System.Console.WriteLine($"  Is Leaf: {cat.IsLeaf}");
                        System.Console.WriteLine($"  Parent ID: {cat.ParentId ?? 0}");
                        System.Console.WriteLine($"  Created: {cat.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                        System.Console.WriteLine($"  Updated: {cat.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                        
                        // Show children if any
                        var children = allCategories.Where(c => c.ParentId == cat.Id).ToList();
                        if (children.Any())
                        {
                            System.Console.WriteLine($"  Children ({children.Count}):");
                            foreach (var child in children.Take(5))
                            {
                                System.Console.WriteLine($"    - {child.Category}");
                            }
                            if (children.Count > 5)
                            {
                                System.Console.WriteLine($"    ... and {children.Count - 5} more");
                            }
                        }
                        
                        System.Console.WriteLine();
                        System.Console.Write("Press any key to continue...");
                        System.Console.ReadKey(true);
                    }
                }
                else if (int.TryParse(input, out var index) && index > 0 && index <= categoriesAtLevel.Count)
                {
                    var selectedCategory = categoriesAtLevel[index - 1];
                    
                    if (selectedCategory.IsLeaf)
                    {
                        ShowInfo($"'{selectedCategory.Category}' is a leaf category (no sub-categories)");
                        System.Console.Write("Press any key to continue...");
                        System.Console.ReadKey(true);
                    }
                    else
                    {
                        // Navigate into this category
                        currentParentId = selectedCategory.Id;
                        breadcrumb.Push((selectedCategory.Id, selectedCategory.Category));
                    }
                }
                else
                {
                    ShowError("Invalid choice. Please try again.");
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error navigating categories: {ex.Message}");
        }
    }
}