using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Console;

public static class TestNoteService
{
    public static async Task RunNoteServiceTests(IServiceProvider services)
    {
        // Create a scope for scoped services
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        
        var logger = scopedServices.GetRequiredService<ILogger<MenuSystem>>();
        var noteService = scopedServices.GetRequiredService<INoteService>();
        
        bool exitRequested = false;
        
        while (!exitRequested)
        {
            System.Console.WriteLine("\n=== Note Service Test Menu ===");
            System.Console.WriteLine("1. Get All Notes");
            System.Console.WriteLine("2. Get Notes by WorkOrder ID");
            System.Console.WriteLine("3. Create Note for WorkOrder");
            System.Console.WriteLine("0. Back to Main Menu");
            System.Console.WriteLine();
            System.Console.Write("Enter your choice: ");
            
            var choice = System.Console.ReadLine()?.Trim();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        await GetAllNotes(noteService, logger);
                        break;
                    case "2":
                        await GetNotesByWorkOrder(noteService, logger);
                        break;
                    case "3":
                        await CreateNoteForWorkOrder(noteService, logger);
                        break;
                    case "0":
                        exitRequested = true;
                        break;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                
                if (!exitRequested && choice != "0")
                {
                    System.Console.WriteLine("\nPress any key to continue...");
                    System.Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"\nError: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
                System.Console.ResetColor();
                
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }
    
    private static async Task GetAllNotes(INoteService noteService, ILogger logger)
    {
        System.Console.WriteLine("\n=== Getting All Notes ===");
        
        logger.LogInformation("Fetching all notes...");
        var response = await noteService.GetNotesAsync();
        
        DisplayNotesResponse(response);
    }
    
    private static async Task GetNotesByWorkOrder(INoteService noteService, ILogger logger)
    {
        System.Console.Write("\nEnter WorkOrder ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            System.Console.WriteLine("Invalid WorkOrder ID");
            return;
        }
        
        logger.LogInformation("Fetching notes for WorkOrder {WorkOrderId}...", workOrderId);
        var response = await noteService.GetNotesByObjectAsync("WorkOrder", workOrderId);
        
        System.Console.WriteLine($"\n=== Notes for WorkOrder #{workOrderId} ===");
        DisplayNotesResponse(response);
    }
    
    private static async Task CreateNoteForWorkOrder(INoteService noteService, ILogger logger)
    {
        System.Console.WriteLine("\n=== Create Note for WorkOrder ===");
        
        System.Console.Write("Enter WorkOrder ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            System.Console.WriteLine("Invalid WorkOrder ID");
            return;
        }
        
        System.Console.Write("Enter Note Content: ");
        var content = System.Console.ReadLine() ?? "";
        
        if (string.IsNullOrWhiteSpace(content))
        {
            System.Console.WriteLine("Note content cannot be empty");
            return;
        }
        
        System.Console.WriteLine("\nVisibility Options:");
        System.Console.WriteLine("1. All (default)");
        System.Console.WriteLine("2. Internal");
        System.Console.WriteLine("3. Private");
        System.Console.Write("Select visibility (1-3) [default: 1]: ");
        var visibilityChoice = System.Console.ReadLine()?.Trim();
        
        string visibility = visibilityChoice switch
        {
            "2" => "internal",
            "3" => "private",
            _ => "all"
        };
        
        System.Console.Write("Action Required? (y/n) [default: n]: ");
        var actionRequired = System.Console.ReadLine()?.ToLower() == "y";
        
        System.Console.WriteLine("\nNote Type Options:");
        System.Console.WriteLine("1. General (default, ID: 2)");
        System.Console.WriteLine("2. System (ID: 1)");
        System.Console.WriteLine("3. Action (ID: 3)");
        System.Console.Write("Select note type (1-3) [default: 1]: ");
        var noteTypeChoice = System.Console.ReadLine()?.Trim();
        
        int noteTypeId = noteTypeChoice switch
        {
            "2" => 1,
            "3" => 3,
            _ => 2
        };
        
        logger.LogInformation("Creating note for WorkOrder {WorkOrderId}...", workOrderId);
        
        try
        {
            var note = await noteService.CreateNoteForWorkOrderAsync(
                workOrderId, 
                content, 
                visibility, 
                actionRequired, 
                noteTypeId
            );
            
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("\n✅ Note created successfully!");
            System.Console.ResetColor();
            
            DisplayNote(note);
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n❌ Failed to create note: {ex.Message}");
            System.Console.ResetColor();
        }
    }
    
    private static void DisplayNotesResponse(PagedResponse<Note> response)
    {
        System.Console.WriteLine($"\nTotal Notes: {response.TotalCount}");
        System.Console.WriteLine($"Page: {response.Page} of {response.TotalPages}");
        System.Console.WriteLine($"Showing {response.Data?.Count ?? 0} notes\n");
        
        if (response.Data == null || !response.Data.Any())
        {
            System.Console.WriteLine("No notes found.");
            return;
        }
        
        foreach (var note in response.Data)
        {
            DisplayNoteSummary(note);
        }
    }
    
    private static void DisplayNoteSummary(Note note)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"Note #{note.Id}");
        System.Console.ResetColor();
        
        System.Console.WriteLine($"  Content: {TruncateString(note.Content, 100)}");
        System.Console.WriteLine($"  Type: {note.NoteType?.Name ?? "N/A"}");
        System.Console.WriteLine($"  Created: {note.CreatedAt:yyyy-MM-dd HH:mm}");
        System.Console.WriteLine($"  User: {note.User?.FullName ?? "Unknown"} ({note.User?.Email ?? "N/A"})");
        
        if (!string.IsNullOrEmpty(note.ObjectType) && note.ObjectId.HasValue)
        {
            System.Console.WriteLine($"  Attached to: {note.ObjectType} #{note.ObjectId}");
        }
        
        if (note.IsPrivate == true)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("  [PRIVATE]");
            System.Console.ResetColor();
        }
        
        if (note.IsInternal == true)
        {
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine("  [INTERNAL]");
            System.Console.ResetColor();
        }
        
        System.Console.WriteLine();
    }
    
    private static void DisplayNote(Note note)
    {
        System.Console.WriteLine("\n=== Note Details ===");
        System.Console.WriteLine($"ID: {note.Id}");
        System.Console.WriteLine($"Content: {note.Content}");
        System.Console.WriteLine($"Type: {note.NoteType?.Name ?? "N/A"}");
        System.Console.WriteLine($"Created: {note.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        System.Console.WriteLine($"Updated: {note.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        
        if (note.User != null)
        {
            System.Console.WriteLine($"User: {note.User.FullName} ({note.User.Email})");
            System.Console.WriteLine($"User ID: {note.User.Id}");
        }
        
        if (!string.IsNullOrEmpty(note.ObjectType) && note.ObjectId.HasValue)
        {
            System.Console.WriteLine($"Attached to: {note.ObjectType} #{note.ObjectId}");
        }
        
        System.Console.WriteLine($"Private: {note.IsPrivate?.ToString() ?? "N/A"}");
        System.Console.WriteLine($"Internal: {note.IsInternal?.ToString() ?? "N/A"}");
    }
    
    private static string TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return "N/A";
            
        if (value.Length <= maxLength)
            return value;
            
        return value.Substring(0, maxLength) + "...";
    }
}