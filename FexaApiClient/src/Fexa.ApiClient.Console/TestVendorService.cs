using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Console;

public class TestVendorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TestVendorService> _logger;
    
    public TestVendorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TestVendorService>>();
    }
    
    public async Task RunTests()
    {
        using var scope = _serviceProvider.CreateScope();
        var vendorService = scope.ServiceProvider.GetRequiredService<IVendorService>();
        
        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("=== Vendor Service Test Menu ===");
            System.Console.WriteLine();
            System.Console.WriteLine("1. Get Vendors (Paginated)");
            System.Console.WriteLine("2. Get Vendor by ID");
            System.Console.WriteLine("3. Get All Vendors (Multiple Pages)");
            System.Console.WriteLine("4. Get Active Vendors");
            System.Console.WriteLine("5. Get Vendors by Compliance Status");
            System.Console.WriteLine("6. Get Assignable Vendors");
            System.Console.WriteLine("7. Search Vendors with Filters");
            System.Console.WriteLine("8. Test Vendor Sorting");
            System.Console.WriteLine("9. Get Vendors by Work Order ID");
            System.Console.WriteLine("0. Back to Main Menu");
            System.Console.WriteLine();
            System.Console.Write("Select an option: ");
            
            var choice = System.Console.ReadLine();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        await TestGetVendors(vendorService);
                        break;
                    case "2":
                        await TestGetVendorById(vendorService);
                        break;
                    case "3":
                        await TestGetAllVendors(vendorService);
                        break;
                    case "4":
                        await TestGetActiveVendors(vendorService);
                        break;
                    case "5":
                        await TestGetVendorsByCompliance(vendorService);
                        break;
                    case "6":
                        await TestGetAssignableVendors(vendorService);
                        break;
                    case "7":
                        await TestSearchVendors(vendorService);
                        break;
                    case "8":
                        await TestVendorSorting(vendorService);
                        break;
                    case "9":
                        await TestGetVendorsByWorkOrderId(vendorService);
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
    
    private async Task TestGetVendors(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Vendors (Paginated) ===");
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
        
        System.Console.WriteLine($"\nFetching vendors (start: {start}, limit: {pageSize})...");
        
        var response = await vendorService.GetVendorsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total Count: {response.TotalCount}");
        System.Console.WriteLine($"Page: {response.Page} of {response.TotalPages}");
        System.Console.WriteLine($"Page Size: {response.PageSize}");
        System.Console.WriteLine($"Has Next Page: {response.HasNextPage}");
        System.Console.WriteLine($"Has Previous Page: {response.HasPreviousPage}");
        System.Console.WriteLine($"Returned Items: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Vendors ===");
            foreach (var vendor in response.Data.Take(5))
            {
                DisplayVendorSummary(vendor);
            }
            
            if (response.Data.Count() > 5)
            {
                System.Console.WriteLine($"\n... and {response.Data.Count() - 5} more");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetVendorById(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Vendor by ID ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter Vendor ID: ");
        var idStr = System.Console.ReadLine();
        
        if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
        {
            System.Console.WriteLine("Invalid ID. Press any key to continue...");
            System.Console.ReadKey();
            return;
        }
        
        System.Console.WriteLine($"\nFetching vendor {id}...");
        
        var vendor = await vendorService.GetVendorAsync(id);
        
        if (vendor != null)
        {
            System.Console.WriteLine("\n=== Vendor Details ===");
            DisplayVendorDetails(vendor);
        }
        else
        {
            System.Console.WriteLine($"\nVendor with ID {id} not found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetAllVendors(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get All Vendors ===");
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
        
        System.Console.WriteLine($"\nFetching all vendors (max {maxPages} pages, {pageSize} per page)...");
        
        var vendors = await vendorService.GetAllVendorsAsync(parameters, maxPages);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total vendors fetched: {vendors.Count}");
        
        if (vendors.Any())
        {
            System.Console.WriteLine("\n=== First 10 Vendors ===");
            foreach (var vendor in vendors.Take(10))
            {
                DisplayVendorSummary(vendor);
            }
            
            if (vendors.Count > 10)
            {
                System.Console.WriteLine($"\n... and {vendors.Count - 10} more");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetActiveVendors(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Active Vendors ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter page size (default 20): ");
        var pageSizeStr = System.Console.ReadLine();
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 20 : int.Parse(pageSizeStr);
        
        var parameters = new QueryParameters
        {
            Limit = pageSize
        };
        
        System.Console.WriteLine($"\nFetching active vendors...");
        
        var response = await vendorService.GetActiveVendorsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total active vendors: {response.TotalCount}");
        System.Console.WriteLine($"Returned: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Active Vendors ===");
            foreach (var vendor in response.Data.Take(10))
            {
                DisplayVendorSummary(vendor);
            }
        }
        else
        {
            System.Console.WriteLine("\nNo active vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetVendorsByCompliance(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Vendors by Compliance Status ===");
        System.Console.WriteLine();
        
        System.Console.Write("Show compliant vendors? (y/n): ");
        var compliant = System.Console.ReadLine()?.ToLower() == "y";
        
        System.Console.Write("Enter page size (default 20): ");
        var pageSizeStr = System.Console.ReadLine();
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 20 : int.Parse(pageSizeStr);
        
        var parameters = new QueryParameters
        {
            Limit = pageSize
        };
        
        System.Console.WriteLine($"\nFetching {(compliant ? "compliant" : "non-compliant")} vendors...");
        
        var response = await vendorService.GetVendorsByComplianceStatusAsync(compliant, parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total {(compliant ? "compliant" : "non-compliant")} vendors: {response.TotalCount}");
        System.Console.WriteLine($"Returned: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine($"\n=== {(compliant ? "Compliant" : "Non-Compliant")} Vendors ===");
            foreach (var vendor in response.Data.Take(10))
            {
                DisplayVendorSummary(vendor);
                System.Console.WriteLine($"  Compliance Met: {vendor.ComplianceRequirementMet}");
            }
        }
        else
        {
            System.Console.WriteLine($"\nNo {(compliant ? "compliant" : "non-compliant")} vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestGetAssignableVendors(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Get Assignable Vendors ===");
        System.Console.WriteLine();
        
        System.Console.Write("Enter page size (default 20): ");
        var pageSizeStr = System.Console.ReadLine();
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 20 : int.Parse(pageSizeStr);
        
        var parameters = new QueryParameters
        {
            Limit = pageSize
        };
        
        System.Console.WriteLine($"\nFetching assignable vendors...");
        
        var response = await vendorService.GetAssignableVendorsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total assignable vendors: {response.TotalCount}");
        System.Console.WriteLine($"Returned: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Assignable Vendors ===");
            foreach (var vendor in response.Data.Take(10))
            {
                DisplayVendorSummary(vendor);
                System.Console.WriteLine($"  Work Orders: {vendor.TotalWorkorderCount}");
                System.Console.WriteLine($"  Score: {vendor.OverallScore}");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo assignable vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestSearchVendors(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Search Vendors with Filters ===");
        System.Console.WriteLine();
        
        System.Console.WriteLine("1. Filter by Active status");
        System.Console.WriteLine("2. Filter by Entity ID");
        System.Console.WriteLine("3. Filter by Date Range");
        System.Console.WriteLine("4. Filter by Overall Score");
        System.Console.WriteLine("5. Custom filter");
        System.Console.Write("\nSelect filter type: ");
        
        var filterChoice = System.Console.ReadLine();
        
        var filters = new List<FexaFilter>();
        
        switch (filterChoice)
        {
            case "1":
                System.Console.Write("Show active vendors only? (y/n): ");
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
                System.Console.Write("Enter minimum overall score: ");
                if (decimal.TryParse(System.Console.ReadLine(), out var minScore))
                {
                    filters.Add(new FexaFilter("overall_score", minScore, "gte"));
                }
                break;
                
            case "5":
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
        
        System.Console.WriteLine($"\nSearching vendors with filters...");
        
        var response = await vendorService.GetVendorsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total matching: {response.TotalCount}");
        System.Console.WriteLine($"Returned: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine("\n=== Matching Vendors ===");
            foreach (var vendor in response.Data.Take(10))
            {
                DisplayVendorSummary(vendor);
            }
        }
        else
        {
            System.Console.WriteLine("\nNo matching vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private async Task TestVendorSorting(IVendorService vendorService)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Test Vendor Sorting ===");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Sort by:");
        System.Console.WriteLine("1. ID");
        System.Console.WriteLine("2. Created Date");
        System.Console.WriteLine("3. Updated Date");
        System.Console.WriteLine("4. Overall Score");
        System.Console.WriteLine("5. Total Work Orders");
        System.Console.Write("\nSelect sort field: ");
        
        var sortChoice = System.Console.ReadLine();
        string sortField = sortChoice switch
        {
            "1" => "id",
            "2" => "created_at",
            "3" => "updated_at",
            "4" => "overall_score",
            "5" => "total_workorder_count",
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
        
        System.Console.WriteLine($"\nFetching vendors sorted by {sortField} ({(sortDesc ? "DESC" : "ASC")})...");
        
        var response = await vendorService.GetVendorsAsync(parameters);
        
        System.Console.WriteLine($"\n=== Results ===");
        System.Console.WriteLine($"Total: {response.TotalCount}");
        System.Console.WriteLine($"Showing: {response.Data?.Count() ?? 0}");
        
        if (response.Data != null && response.Data.Any())
        {
            System.Console.WriteLine($"\n=== Vendors (sorted by {sortField}) ===");
            foreach (var vendor in response.Data)
            {
                var sortValue = sortField switch
                {
                    "id" => vendor.Id.ToString(),
                    "created_at" => vendor.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                    "updated_at" => vendor.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                    "overall_score" => vendor.OverallScore?.ToString() ?? "N/A",
                    "total_workorder_count" => vendor.TotalWorkorderCount?.ToString() ?? "N/A",
                    _ => "N/A"
                };
                
                System.Console.WriteLine($"ID: {vendor.Id,-8} | {sortField}: {sortValue,-20} | Active: {vendor.Active}");
            }
        }
        else
        {
            System.Console.WriteLine("\nNo vendors found.");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
    
    private void DisplayVendorSummary(Vendor vendor)
    {
        System.Console.WriteLine($"\nVendor ID: {vendor.Id}");
        System.Console.WriteLine($"  Entity ID: {vendor.EntityId}");
        System.Console.WriteLine($"  Active: {vendor.Active}");
        System.Console.WriteLine($"  Assignable: {vendor.Assignable}");
        System.Console.WriteLine($"  Compliance Met: {vendor.ComplianceRequirementMet}");
        System.Console.WriteLine($"  Created: {vendor.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"  Updated: {vendor.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        
        if (vendor.DefaultDispatchAddress != null)
        {
            System.Console.WriteLine($"  Company: {vendor.DefaultDispatchAddress.Company ?? "N/A"}");
            System.Console.WriteLine($"  Location: {vendor.DefaultDispatchAddress.City}, {vendor.DefaultDispatchAddress.State}");
        }
        
        if (vendor.Organization != null)
        {
            System.Console.WriteLine($"  Organization ID: {vendor.Organization.Id}");
            System.Console.WriteLine($"  EIN: {vendor.Organization.Ein ?? "N/A"}");
        }
        
        System.Console.WriteLine($"  Work Orders: {vendor.TotalWorkorderCount}");
        System.Console.WriteLine($"  Overall Score: {vendor.OverallScore}");
    }
    
    private void DisplayVendorDetails(Vendor vendor)
    {
        System.Console.WriteLine($"Vendor ID: {vendor.Id}");
        System.Console.WriteLine($"Entity ID: {vendor.EntityId}");
        System.Console.WriteLine($"Active: {vendor.Active}");
        System.Console.WriteLine($"Assignable: {vendor.Assignable}");
        System.Console.WriteLine($"Created: {vendor.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"Updated: {vendor.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        System.Console.WriteLine($"Start Date: {vendor.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"End Date: {vendor.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"IVR ID: {vendor.IvrId ?? "N/A"}");
        System.Console.WriteLine($"Auto Accept: {vendor.AutoAccept ?? "N/A"}");
        System.Console.WriteLine($"Total Work Orders: {vendor.TotalWorkorderCount}");
        System.Console.WriteLine($"Overall Score: {vendor.OverallScore}");
        System.Console.WriteLine($"Compliance Requirement Met: {vendor.ComplianceRequirementMet}");
        System.Console.WriteLine($"Discount Invoicing: {vendor.DiscountInvoicing}");
        System.Console.WriteLine($"Opts Out of Mass Dispatches: {vendor.OptsOutOfMassDispatches}");
        System.Console.WriteLine($"Distributor: {vendor.Distributor}");
        
        if (vendor.DefaultDispatchAddress != null)
        {
            System.Console.WriteLine("\n=== Dispatch Address ===");
            var addr = vendor.DefaultDispatchAddress;
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
            System.Console.WriteLine($"  Mobile: {addr.Mobile ?? "N/A"}");
            System.Console.WriteLine($"  Emergency: {addr.Emergency ?? "N/A"}");
            System.Console.WriteLine($"  Email: {addr.Email ?? "N/A"}");
            System.Console.WriteLine($"  Timezone: {addr.Timezone ?? "N/A"}");
        }
        
        if (vendor.DefaultBillingAddress != null)
        {
            System.Console.WriteLine("\n=== Billing Address ===");
            var addr = vendor.DefaultBillingAddress;
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
        
        if (vendor.DefaultShippingAddress != null)
        {
            System.Console.WriteLine("\n=== Shipping Address ===");
            var addr = vendor.DefaultShippingAddress;
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
        
        if (vendor.Organization != null)
        {
            System.Console.WriteLine("\n=== Organization ===");
            System.Console.WriteLine($"  ID: {vendor.Organization.Id}");
            System.Console.WriteLine($"  EIN: {vendor.Organization.Ein ?? "N/A"}");
            System.Console.WriteLine($"  Taxable: {vendor.Organization.Taxable}");
            System.Console.WriteLine($"  Is 1099: {vendor.Organization.Is1099}");
            System.Console.WriteLine($"  Tax Classification ID: {vendor.Organization.TaxClassificationId}");
            System.Console.WriteLine($"  Accounting ID: {vendor.Organization.AccountingId ?? "N/A"}");
        }
        
        if (vendor.CustomFieldValues != null && vendor.CustomFieldValues.Any())
        {
            System.Console.WriteLine("\n=== Custom Fields ===");
            foreach (var kvp in vendor.CustomFieldValues)
            {
                System.Console.WriteLine($"  {kvp.Key}: {kvp.Value ?? "N/A"}");
            }
        }
        
        if (vendor.ObjectState != null)
        {
            System.Console.WriteLine("\n=== Current Status ===");
            System.Console.WriteLine($"  Status ID: {vendor.ObjectState.StatusId}");
            if (vendor.ObjectState.Status != null)
            {
                System.Console.WriteLine($"  Status Name: {vendor.ObjectState.Status.Name}");
                if (vendor.ObjectState.Status.WorkflowType != null)
                {
                    System.Console.WriteLine($"  Workflow Type: {vendor.ObjectState.Status.WorkflowType.Name}");
                }
            }
            System.Console.WriteLine($"  Status Changed: {vendor.ObjectState.StatusLastChanged?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        }
    }
    
    private async Task TestGetVendorsByWorkOrderId(IVendorService vendorService)
    {
        System.Console.Write("Enter Work Order ID: ");
        var input = System.Console.ReadLine();
        
        if (!int.TryParse(input, out var workOrderId))
        {
            System.Console.WriteLine("Invalid Work Order ID.");
            System.Console.WriteLine("\nPress any key to continue...");
            System.Console.ReadKey();
            return;
        }
        
        System.Console.WriteLine($"\nFetching vendors for work order {workOrderId}...");
        
        try
        {
            var vendors = await vendorService.GetVendorsByWorkOrderIdAsync(workOrderId);
            
            System.Console.WriteLine($"\n=== Results ===");
            System.Console.WriteLine($"Found {vendors.Count} vendor(s) assigned to work order {workOrderId}");
            
            if (vendors.Any())
            {
                System.Console.WriteLine("\n=== Assigned Vendors ===");
                foreach (var vendor in vendors)
                {
                    System.Console.WriteLine($"\n--- Vendor {vendor.Id} ---");
                    DisplayVendorSummary(vendor);
                    System.Console.WriteLine();
                }
                
                System.Console.Write("\nShow detailed view for a vendor? (y/n): ");
                if (System.Console.ReadLine()?.ToLower() == "y")
                {
                    System.Console.Write("Enter Vendor ID: ");
                    if (int.TryParse(System.Console.ReadLine(), out var vendorId))
                    {
                        var vendor = vendors.FirstOrDefault(v => v.Id == vendorId);
                        if (vendor != null)
                        {
                            System.Console.WriteLine("\n=== Detailed Vendor Information ===");
                            DisplayVendorDetails(vendor);
                        }
                        else
                        {
                            System.Console.WriteLine("Vendor not found in the list.");
                        }
                    }
                }
            }
            else
            {
                System.Console.WriteLine("\nNo vendors are assigned to this work order.");
                System.Console.WriteLine("Note: This could mean the work order doesn't exist or has no assignments.");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            System.Console.WriteLine($"\nWork order {workOrderId} not found.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nError: {ex.Message}");
        }
        
        System.Console.WriteLine("\nPress any key to continue...");
        System.Console.ReadKey();
    }
}