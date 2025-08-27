using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Console;

public class TestCreateWorkOrder
{
    public static async Task RunCreateWorkOrderTest(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<TestCreateWorkOrder>>();
        
        System.Console.Clear();
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("\n>>> Create Work Order Test <<<");
        System.Console.ResetColor();
        System.Console.WriteLine("──────────────────────────────");
        
        using var scope = services.CreateScope();
        var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();
        var cachedClientService = services.GetRequiredService<ICachedClientService>();
        var locationService = services.GetRequiredService<ILocationService>();
        var categoryService = services.GetRequiredService<IWorkOrderCategoryService>();
        var priorityService = services.GetRequiredService<IPriorityService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        
        try
        {
            // Step 1: Select Client
            System.Console.WriteLine("\n=== Step 1: Select Client ===");
            System.Console.Write("Enter client search term (or press Enter to list all): ");
            var clientSearch = System.Console.ReadLine();
            
            List<ClientInfo> clients;
            if (!string.IsNullOrEmpty(clientSearch))
            {
                clients = await cachedClientService.SearchClientInfoAsync(clientSearch);
            }
            else
            {
                clients = await cachedClientService.GetActiveClientInfoAsync();
            }
            
            if (!clients.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("No clients found.");
                System.Console.ResetColor();
                return;
            }
            
            System.Console.WriteLine($"\nFound {clients.Count} client(s):");
            for (int i = 0; i < Math.Min(clients.Count, 10); i++)
            {
                System.Console.WriteLine($"{i + 1}. {clients[i].Name} (ID: {clients[i].Id})");
            }
            
            System.Console.Write("\nSelect client number: ");
            if (!int.TryParse(System.Console.ReadLine(), out var clientIndex) || 
                clientIndex < 1 || clientIndex > clients.Count)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Invalid selection.");
                System.Console.ResetColor();
                return;
            }
            
            var selectedClient = clients[clientIndex - 1];
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Selected: {selectedClient.Name}");
            System.Console.ResetColor();
            
            // Step 2: Select Location/Facility
            System.Console.WriteLine("\n=== Step 2: Select Location/Facility ===");
            var locations = await locationService.GetLocationsByClientAsync(selectedClient.Id);
            
            if (!locations.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"No locations found for client {selectedClient.Name}.");
                System.Console.ResetColor();
                return;
            }
            
            System.Console.WriteLine($"\nFound {locations.Count} location(s):");
            for (int i = 0; i < Math.Min(locations.Count, 20); i++)
            {
                var loc = locations[i];
                System.Console.WriteLine($"{i + 1}. {loc.Name} - {loc.City}, {loc.State} (ID: {loc.Id})");
            }
            
            System.Console.Write("\nSelect location number: ");
            if (!int.TryParse(System.Console.ReadLine(), out var locationIndex) || 
                locationIndex < 1 || locationIndex > locations.Count)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Invalid selection.");
                System.Console.ResetColor();
                return;
            }
            
            var selectedLocation = locations[locationIndex - 1];
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Selected: {selectedLocation.Name}");
            System.Console.ResetColor();
            
            // Step 3: Select Category
            System.Console.WriteLine("\n=== Step 3: Select Work Order Category ===");
            var categories = await categoryService.GetAllCategoriesAsync();
            categories = categories.Where(c => c.Active).ToList();
            var leafCategories = categories.Where(c => c.IsLeaf).OrderBy(c => c.CategoryWithAllAncestors ?? c.Category).ToList();
            
            System.Console.WriteLine($"\nShowing {Math.Min(leafCategories.Count, 20)} leaf categories:");
            for (int i = 0; i < Math.Min(leafCategories.Count, 20); i++)
            {
                var cat = leafCategories[i];
                System.Console.WriteLine($"{i + 1}. {cat.CategoryWithAllAncestors ?? cat.Category} (ID: {cat.Id})");
            }
            
            System.Console.Write("\nSelect category number (or enter 0 to search): ");
            var catInput = System.Console.ReadLine();
            
            WorkOrderCategory? selectedCategory = null;
            if (catInput == "0")
            {
                System.Console.Write("Enter category search term: ");
                var catSearch = System.Console.ReadLine()?.ToLower();
                var filtered = leafCategories.Where(c => 
                    (c.CategoryWithAllAncestors ?? c.Category).ToLower().Contains(catSearch ?? "")).ToList();
                
                if (filtered.Any())
                {
                    for (int i = 0; i < Math.Min(filtered.Count, 10); i++)
                    {
                        System.Console.WriteLine($"{i + 1}. {filtered[i].CategoryWithAllAncestors ?? filtered[i].Category} (ID: {filtered[i].Id})");
                    }
                    System.Console.Write("Select category: ");
                    if (int.TryParse(System.Console.ReadLine(), out var idx) && 
                        idx >= 1 && idx <= filtered.Count)
                    {
                        selectedCategory = filtered[idx - 1];
                    }
                }
            }
            else if (int.TryParse(catInput, out var catIndex) && 
                     catIndex >= 1 && catIndex <= leafCategories.Count)
            {
                selectedCategory = leafCategories[catIndex - 1];
            }
            
            if (selectedCategory == null)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Invalid category selection.");
                System.Console.ResetColor();
                return;
            }
            
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Selected: {selectedCategory.CategoryWithAllAncestors ?? selectedCategory.Category}");
            System.Console.ResetColor();
            
            // Step 4: Select Priority
            System.Console.WriteLine("\n=== Step 4: Select Priority ===");
            var priorities = await priorityService.GetActivePrioritiesAsync();
            
            System.Console.WriteLine("\nAvailable priorities:");
            for (int i = 0; i < priorities.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {priorities[i].Name} (ID: {priorities[i].Id})");
            }
            
            System.Console.Write("\nSelect priority number: ");
            if (!int.TryParse(System.Console.ReadLine(), out var priorityIndex) || 
                priorityIndex < 1 || priorityIndex > priorities.Count)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Invalid selection.");
                System.Console.ResetColor();
                return;
            }
            
            var selectedPriority = priorities[priorityIndex - 1];
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Selected: {selectedPriority.Name}");
            System.Console.ResetColor();
            
            // Step 5: Enter Description
            System.Console.WriteLine("\n=== Step 5: Enter Work Order Description ===");
            System.Console.WriteLine("Enter description (required):");
            var description = System.Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(description))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Description is required.");
                System.Console.ResetColor();
                return;
            }
            
            // Step 6: Optional - Client PO Number
            System.Console.WriteLine("\n=== Step 6: Optional - Client PO Number ===");
            System.Console.Write("Enter Client PO Number (or press Enter to skip): ");
            var poNumber = System.Console.ReadLine();
            
            // Step 7: Get current user ID for placed_by
            System.Console.WriteLine("\n=== Step 7: Select User (placed_by) ===");
            System.Console.WriteLine("Fetching users...");
            var usersResponse = await userService.GetUsersAsync(new QueryParameters { Limit = 10 });
            
            if (usersResponse.Data == null || !usersResponse.Data.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("No users found. Cannot proceed.");
                System.Console.ResetColor();
                return;
            }
            
            System.Console.WriteLine("\nAvailable users:");
            var usersList = usersResponse.Data.ToList();
            for (int i = 0; i < usersList.Count; i++)
            {
                var user = usersList[i];
                System.Console.WriteLine($"{i + 1}. {user.FirstName} {user.LastName} ({user.Email}) - ID: {user.Id}");
            }
            
            System.Console.Write("\nSelect user number: ");
            if (!int.TryParse(System.Console.ReadLine(), out var userIndex) || 
                userIndex < 1 || userIndex > usersList.Count)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Invalid selection.");
                System.Console.ResetColor();
                return;
            }
            
            var selectedUser = usersList[userIndex - 1];
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Selected: {selectedUser.FirstName} {selectedUser.LastName}");
            System.Console.ResetColor();
            
            // Step 8: Confirm and Create
            System.Console.WriteLine("\n=== Work Order Summary ===");
            System.Console.WriteLine($"Client: {selectedClient.Name}");
            System.Console.WriteLine($"Location: {selectedLocation.Name}");
            System.Console.WriteLine($"Category: {selectedCategory.CategoryWithAllAncestors ?? selectedCategory.Category}");
            System.Console.WriteLine($"Priority: {selectedPriority.Name}");
            System.Console.WriteLine($"Description: {description}");
            System.Console.WriteLine($"Placed By: {selectedUser.FirstName} {selectedUser.LastName}");
            if (!string.IsNullOrEmpty(poNumber))
            {
                System.Console.WriteLine($"Client PO: {poNumber}");
            }
            
            System.Console.Write("\nConfirm creation? (y/n): ");
            if (System.Console.ReadLine()?.ToLower() != "y")
            {
                System.Console.WriteLine("Creation cancelled.");
                return;
            }
            
            // Create the work order
            System.Console.WriteLine("\nCreating work order...");
            
            var request = new CreateWorkOrderRequest
            {
                Workorders = new WorkOrderData
                {
                    WorkorderClassId = 1, // Default to 1 (standard work order class)
                    PriorityId = selectedPriority.Id,
                    CategoryId = selectedCategory.Id,
                    Description = description ?? "",
                    FacilityId = selectedLocation.Id,
                    PlacedBy = int.Parse(selectedUser.Id),
                    PlacedFor = selectedClient.Id,
                    ClientPurchaseOrderNumbers = string.IsNullOrEmpty(poNumber) 
                        ? new List<ClientPurchaseOrder>()
                        : new List<ClientPurchaseOrder> 
                        { 
                            new ClientPurchaseOrder 
                            { 
                                PurchaseOrderNumber = poNumber, 
                                Active = true 
                            } 
                        }
                }
            };
            
            var createdWorkOrder = await workOrderService.CreateWorkOrderAsync(request);
            
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"\n✅ Successfully created work order!");
            System.Console.ResetColor();
            
            System.Console.WriteLine($"\nWork Order Details:");
            System.Console.WriteLine($"  ID: {createdWorkOrder.Id}");
            System.Console.WriteLine($"  Number: {createdWorkOrder.WorkOrderNumber ?? "N/A"}");
            System.Console.WriteLine($"  Status: {createdWorkOrder.Status}");
            System.Console.WriteLine($"  Created At: {createdWorkOrder.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            
            System.Console.Write("\nView full work order details? (y/n): ");
            if (System.Console.ReadLine()?.ToLower() == "y")
            {
                var fullWorkOrder = await workOrderService.GetWorkOrderAsync(createdWorkOrder.Id);
                
                System.Console.WriteLine("\n=== Full Work Order Details ===");
                System.Console.WriteLine($"ID: {fullWorkOrder.Id}");
                System.Console.WriteLine($"Work Order Number: {fullWorkOrder.WorkOrderNumber ?? "N/A"}");
                System.Console.WriteLine($"Description: {fullWorkOrder.Description}");
                System.Console.WriteLine($"Status: {fullWorkOrder.Status}");
                System.Console.WriteLine($"Priority ID: {fullWorkOrder.PriorityId}");
                // Category ID is not available in the work order response
                System.Console.WriteLine($"Client: {fullWorkOrder.ClientName} (ID: {fullWorkOrder.ClientId})");
                System.Console.WriteLine($"Store: {fullWorkOrder.StoreName}");
                System.Console.WriteLine($"Address: {fullWorkOrder.StoreAddress}, {fullWorkOrder.StoreCity}, {fullWorkOrder.StoreState} {fullWorkOrder.StoreZip}");
                System.Console.WriteLine($"Created: {fullWorkOrder.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                System.Console.WriteLine($"Updated: {fullWorkOrder.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                
                // Client PO Numbers are not available in the work order response
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n❌ Error: {ex.Message}");
            System.Console.ResetColor();
            
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}