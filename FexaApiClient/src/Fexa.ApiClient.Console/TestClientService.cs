using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Console;

public class TestClientService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TestClientService> _logger;
    
    public TestClientService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TestClientService>>();
    }
    
    public async Task RunTests()
    {
        using var scope = _serviceProvider.CreateScope();
        var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
        
        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("=== Client Service Test Menu ===");
            System.Console.WriteLine();
            System.Console.WriteLine("1. Get Clients (Paginated)");
            System.Console.WriteLine("2. Get Client by ID");
            System.Console.WriteLine("3. Get All Clients (Multiple Pages)");
            System.Console.WriteLine("4. Search Clients with Filters");
            System.Console.WriteLine("5. Test Client Sorting");
            System.Console.WriteLine("0. Back to Main Menu");
            System.Console.WriteLine();
            System.Console.Write("Select an option: ");
            
            var choice = System.Console.ReadLine();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        await TestGetClients(clientService);
                        break;
                    case "2":
                        await TestGetClientById(clientService);
                        break;
                    case "3":
                        await TestGetAllClients(clientService);
                        break;
                    case "4":
                        await TestSearchClients(clientService);
                        break;
                    case "5":
                        await TestClientSorting(clientService);
                        break;
                    case "0":
                        return;
                    default:
                        System.Console.WriteLine("Invalid option. Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nError: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }
    
    private async Task TestGetClients(IClientService clientService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Clients (Paginated) ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter page size (default 10): ");
        var pageSizeStr = System.Console.ReadLine();
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 10 : int.Parse(pageSizeStr);
        
        System.Console.Write("Enter starting position (default 0): ");
        var startStr = System.Console.ReadLine();
        var start = string.IsNullOrEmpty(startStr) ? 0 : int.Parse(startStr);
        
        var parameters = new QueryParameters
        {
            Start = start,
            Limit = pageSize
        };
        
        System.Console.WriteLine($"\nFetching clients (start: {start}, limit: {pageSize})...");
        
        var response = await clientService.GetClientsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total Count: {response.TotalCount}");
        System.Console.WriteLine($"Page: {response.Page} of {response.TotalPages}");
        System.Console.WriteLine($"Page Size: {response.PageSize}");
        System.Console.WriteLine($"Has Next Page: {response.HasNextPage}");
        System.Console.WriteLine($"Has Previous Page: {response.HasPreviousPage}");
        System.Console.WriteLine($"Returned Items: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Clients ===");
            foreach (var client in response.Data.Take(5))
            {
                DisplayClientSummary(client);
            }
            
            if (response.Data.Count() > 5)
            {
                System.Console.WriteLine($"\n... and {response.Data.Count() - 5} more");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo clients found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetClientById(IClientService clientService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Client by ID ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter Client ID: ");
        var idStr = System.Console.ReadLine();
        
        if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
        {
            System.Console.WriteLine("Invalid ID. Press any key to continue...");
            System.Console.ReadKey();
            return;
        }
        
        System.Console.WriteLine($"\nFetching client {id}...");
        
        var client = await clientService.GetClientAsync(id);
        
        if (client != null)
        {
            System.Console.WriteLine("\n=== Client Details ===");
            DisplayClientDetails(client);
        }
        else
        {
            System.Console.WriteLine($"\nClient with ID {id} not found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetAllClients(IClientService clientService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get All Clients ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter max pages to fetch (default 2): ");
        var maxPagesStr = System.Console.ReadLine();
        var maxPages = string.IsNullOrEmpty(maxPagesStr) ? 2 : int.Parse(maxPagesStr);
        
        System.Console.Write("Enter page size (default 50): ");
        var pageSizeStr = System.Console.ReadLine();
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 50 : int.Parse(pageSizeStr);
        
        var parameters = new QueryParameters
        {
            Limit = pageSize
        };
        
        System.Console.WriteLine($"\nFetching all clients (max {maxPages} pages, {pageSize} per page)...");
        
        var clients = await clientService.GetAllClientsAsync(parameters, maxPages);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total clients fetched: {clients.Count}");
        
        if (clients.Any())
        {
            System.Console.WriteLine("\n=== First 10 Clients ===");
            foreach (var client in clients.Take(10))
            {
                DisplayClientSummary(client);
            }
            
            if (clients.Count > 10)
            {
                System.Console.WriteLine($"\n... and {clients.Count - 10} more");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo clients found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestSearchClients(IClientService clientService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Search Clients with Filters ===");
        System.Console.WriteLine();
        
        System.Console.WriteLine("1. Filter by Active status");
        System.Console.WriteLine("2. Filter by Entity ID");
        System.Console.WriteLine("3. Filter by Date Range");
        System.Console.WriteLine("4. Custom filter");
        System.Console.Write("\nSelect filter type: ");
        
        var filterChoice = System.Console.ReadLine();
        
        var filters = new List<FexaFilter>();
        
        switch (filterChoice)
        {
            case "1":
                System.Console.Write("Show active clients only? (y/n): ");
                var activeChoice = System.Console.ReadLine()?.ToLower() == "y";
                filters.Add(new FexaFilter("active", activeChoice));
                break;
                
            case "2":
                System.Console.Write("Enter Entity ID: ");
                if (int.TryParse(System.Console.ReadLine(), out var entityId))
                {
                    filters.Add(new FexaFilter("entity_id", entityId));
                }
                break;
                
            case "3":
                System.Console.Write("Enter start date (yyyy-MM-dd): ");
                var startDateStr = System.Console.ReadLine();
                System.Console.Write("Enter end date (yyyy-MM-dd): ");
                var endDateStr = System.Console.ReadLine();
                
                if (DateTime.TryParse(startDateStr, out var startDate) && 
                    DateTime.TryParse(endDateStr, out var endDate))
                {
                    filters.Add(new FexaFilter("created_at", 
                        new[] { startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd") }, 
                        "between"));
                }
                break;
                
            case "4":
                System.Console.Write("Enter field name: ");
                var field = System.Console.ReadLine();
                System.Console.Write("Enter operator (equals, not_equals, in, between, etc.): ");
                var op = System.Console.ReadLine();
                System.Console.Write("Enter value: ");
                var value = System.Console.ReadLine();
                
                if (!string.IsNullOrEmpty(field) && !string.IsNullOrEmpty(value))
                {
                    filters.Add(new FexaFilter(field, value, op ?? "equals"));
                }
                break;
        }
        
        var parameters = new QueryParameters
        {
            Limit = 20,
            Filters = filters
        };
        
        System.Console.WriteLine($"\nSearching clients with filters...");
        
        var response = await clientService.GetClientsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total matching: {response.TotalCount}");
        System.Console.WriteLine($"Returned: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Matching Clients ===");
            foreach (var client in response.Data.Take(10))
            {
                DisplayClientSummary(client);
            }
        }
        else
        {
            System.Console.WriteLine("\nNo matching clients found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestClientSorting(IClientService clientService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Client Sorting ===");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Sort by:");
        System.Console.WriteLine("1. ID");
        System.Console.WriteLine("2. Created Date");
        System.Console.WriteLine("3. Updated Date");
        System.Console.WriteLine("4. Active Status");
        System.Console.Write("\nSelect sort field: ");
        
        var sortChoice = System.Console.ReadLine();
        string sortField = sortChoice switch
        {
            "1" => "id",
            "2" => "created_at",
            "3" => "updated_at",
            "4" => "active",
            _ => "id"
        };
        
        System.Console.Write("Sort descending? (y/n): ");
        var sortDesc = System.Console.ReadLine()?.ToLower() == "y";
        
        var parameters = new QueryParameters
        {
            Limit = 10,
            SortBy = sortField,
            SortDescending = sortDesc
        };
        
        System.Console.WriteLine($"\nFetching clients sorted by {sortField} ({(sortDesc ? "DESC" : "ASC")})...");
        
        var response = await clientService.GetClientsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total: {response.TotalCount}");
        System.Console.WriteLine($"Showing: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine($"\n=== Clients (sorted by {sortField}) ===");
            foreach (var client in response.Data)
            {
                var sortValue = sortField switch
                {
                    "id" => client.Id.ToString(),
                    "created_at" => client.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                    "updated_at" => client.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                    "active" => client.Active.ToString(),
                    _ => "N/A"
                };
                
                System.Console.WriteLine($"ID: {client.Id,-8} | {sortField}: {sortValue,-20} | Active: {client.Active}");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo clients found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private void DisplayClientSummary(Client client)
    {
        System.Console.WriteLine($"\nClient ID: {client.Id}");
        System.Console.WriteLine($"  Entity ID: {client.EntityId}");
        System.Console.WriteLine($"  Active: {client.Active}");
        System.Console.WriteLine($"  Created: {client.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"  Updated: {client.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        
        if (client.DefaultGeneralAddress != null)
        {
            System.Console.WriteLine($"  Company: {client.DefaultGeneralAddress.Company ?? "N/A"}");
            System.Console.WriteLine($"  Location: {client.DefaultGeneralAddress.City}, {client.DefaultGeneralAddress.State}");
        }
        
        if (client.Organization != null)
        {
            System.Console.WriteLine($"  Organization ID: {client.Organization.Id}");
        }
        
        System.Console.WriteLine($"  Work Orders: {client.TotalWorkOrderCount}");
    }
    
    private void DisplayClientDetails(Client client)
    {
        System.Console.WriteLine($"Client ID: {client.Id}");
        System.Console.WriteLine($"Entity ID: {client.EntityId}");
        System.Console.WriteLine($"Active: {client.Active}");
        System.Console.WriteLine($"Created: {client.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"Updated: {client.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"Start Date: {client.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"End Date: {client.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"IVR ID: {client.IvrId ?? "N/A"}");
        System.Console.WriteLine($"Auto Accept: {client.AutoAccept ?? "N/A"}");
        System.Console.WriteLine($"Assignable: {client.Assignable}");
        System.Console.WriteLine($"Total Work Orders: {client.TotalWorkOrderCount}");
        System.Console.WriteLine($"Discount Invoicing: {client.DiscountInvoicing}");
        System.Console.WriteLine($"Opts Out of Mass Dispatches: {client.OptsOutOfMassDispatches}");
        System.Console.WriteLine($"Distributor: {client.Distributor}");
        
        if (client.DefaultGeneralAddress != null)
        {
            System.Console.WriteLine("\n=== General Address ===");
            var addr = client.DefaultGeneralAddress;
            System.Console.WriteLine($"  Company: {addr.Company ?? "N/A"}");
            System.Console.WriteLine($"  DBA: {addr.Dba ?? "N/A"}");
            System.Console.WriteLine($"  Address: {addr.Address1 ?? "N/A"}");
            if (!string.IsNullOrEmpty(addr.Address2))
                System.Console.WriteLine($"          {addr.Address2}");
            System.Console.WriteLine($"  City: {addr.City ?? "N/A"}");
            System.Console.WriteLine($"  State: {addr.State ?? "N/A"}");
            System.Console.WriteLine($"  Postal Code: {addr.PostalCode ?? "N/A"}");
            System.Console.WriteLine($"  Country: {addr.Country ?? "N/A"}");
            System.Console.WriteLine($"  Phone: {addr.Phone ?? "N/A"}");
            System.Console.WriteLine($"  Email: {addr.Email ?? "N/A"}");
            System.Console.WriteLine($"  Timezone: {addr.Timezone ?? "N/A"}");
        }
        
        if (client.DefaultBillingAddress != null)
        {
            System.Console.WriteLine("\n=== Billing Address ===");
            var addr = client.DefaultBillingAddress;
            System.Console.WriteLine($"  Company: {addr.Company ?? "N/A"}");
            System.Console.WriteLine($"  DBA: {addr.Dba ?? "N/A"}");
            System.Console.WriteLine($"  Address: {addr.Address1 ?? "N/A"}");
            if (!string.IsNullOrEmpty(addr.Address2))
                System.Console.WriteLine($"          {addr.Address2}");
            System.Console.WriteLine($"  City: {addr.City ?? "N/A"}");
            System.Console.WriteLine($"  State: {addr.State ?? "N/A"}");
            System.Console.WriteLine($"  Postal Code: {addr.PostalCode ?? "N/A"}");
            System.Console.WriteLine($"  Country: {addr.Country ?? "N/A"}");
        }
        
        if (client.Organization != null)
        {
            System.Console.WriteLine("\n=== Organization ===");
            System.Console.WriteLine($"  ID: {client.Organization.Id}");
            System.Console.WriteLine($"  EIN: {client.Organization.Ein ?? "N/A"}");
            System.Console.WriteLine($"  Taxable: {client.Organization.Taxable}");
            System.Console.WriteLine($"  Is 1099: {client.Organization.Is1099}");
        }
        
        if (client.CustomFieldValues != null)
        {
            System.Console.WriteLine("\n=== Custom Fields ===");
            System.Console.WriteLine($"  CMMS ID: {client.CustomFieldValues.CmmsId ?? "N/A"}");
            System.Console.WriteLine($"  CMMS Program: {client.CustomFieldValues.CmmsProg ?? "N/A"}");
            System.Console.WriteLine($"  Salesperson: {client.CustomFieldValues.Salesperson ?? "N/A"}");
            System.Console.WriteLine($"  Client Relations: {client.CustomFieldValues.ClientRelations ?? "N/A"}");
            System.Console.WriteLine($"  Invoicing Method: {client.CustomFieldValues.InvoicingMethod ?? "N/A"}");
        }
    }
}