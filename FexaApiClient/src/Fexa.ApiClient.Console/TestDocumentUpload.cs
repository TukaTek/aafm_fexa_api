using Microsoft.Extensions.DependencyInjection;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text;

namespace Fexa.ApiClient.Console;

public static class TestDocumentUpload
{
    public static async Task RunDocumentUploadTests(IServiceProvider services)
    {
        System.Console.Clear();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║         Document Upload Service Test       ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.ResetColor();
        System.Console.WriteLine();
        
        var documentService = services.GetRequiredService<IDocumentService>();
        
        while (true)
        {
            System.Console.WriteLine("\n=== Document Upload Test Menu ===");
            System.Console.WriteLine("1. Upload test file to work order");
            System.Console.WriteLine("2. Upload file from path to work order");
            System.Console.WriteLine("3. Create and upload sample PDF");
            System.Console.WriteLine("0. Back to main menu");
            System.Console.WriteLine();
            System.Console.Write("Enter your choice: ");
            
            var choice = System.Console.ReadLine()?.Trim();
            
            switch (choice)
            {
                case "1":
                    await UploadTestFile(documentService);
                    break;
                case "2":
                    await UploadFileFromPath(documentService);
                    break;
                case "3":
                    await UploadSamplePdf(documentService);
                    break;
                case "0":
                    return;
                default:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Invalid choice. Please try again.");
                    System.Console.ResetColor();
                    break;
            }
            
            if (choice != "0")
            {
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }
    }
    
    private static async Task UploadTestFile(IDocumentService documentService)
    {
        System.Console.WriteLine("\n=== Upload Test File ===");
        
        System.Console.Write("Enter Work Order ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid work order ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Document Type ID (e.g., 55): ");
        if (!int.TryParse(System.Console.ReadLine(), out var documentTypeId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid document type ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Description: ");
        var description = System.Console.ReadLine() ?? "Test document upload";
        
        try
        {
            System.Console.WriteLine("\nCreating test text file...");
            
            // Create a test text file content
            var content = $"Test Document Upload\n\nWork Order ID: {workOrderId}\nCreated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nDescription: {description}";
            var bytes = Encoding.UTF8.GetBytes(content);
            var fileName = $"test_document_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            
            System.Console.WriteLine($"Uploading {fileName} ({bytes.Length} bytes)...");
            
            var response = await documentService.AddDocumentToWorkOrderAsync(
                workOrderId,
                documentTypeId,
                description,
                bytes,
                fileName,
                "text/plain");
            
            if (response.Success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✓ Document uploaded successfully!");
                System.Console.ResetColor();
                
                var document = response.Document ?? response.Documents?.FirstOrDefault();
                if (document != null)
                {
                    System.Console.WriteLine($"\nDocument ID: {document.Id}");
                    System.Console.WriteLine($"File Name: {document.FileName ?? document.Filename}");
                    System.Console.WriteLine($"File Size: {document.FileSize} bytes");
                    System.Console.WriteLine($"URL: {document.Url}");
                    System.Console.WriteLine($"Created At: {document.CreatedAt}");
                }
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Upload failed: {response.Message}");
                if (response.Errors != null)
                {
                    System.Console.WriteLine($"  - {response.Errors}");
                }
                System.Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.ResetColor();
        }
    }
    
    private static async Task UploadFileFromPath(IDocumentService documentService)
    {
        System.Console.WriteLine("\n=== Upload File from Path ===");
        
        System.Console.Write("Enter Work Order ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid work order ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Document Type ID (e.g., 55): ");
        if (!int.TryParse(System.Console.ReadLine(), out var documentTypeId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid document type ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter File Path: ");
        var filePath = System.Console.ReadLine()?.Trim();
        
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("File not found");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Description: ");
        var description = System.Console.ReadLine() ?? Path.GetFileName(filePath);
        
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);
            
            System.Console.WriteLine($"\nUploading {fileName} ({fileInfo.Length:N0} bytes)...");
            
            using var fileStream = File.OpenRead(filePath);
            var response = await documentService.AddDocumentToWorkOrderAsync(
                workOrderId,
                documentTypeId,
                description,
                fileStream,
                fileName);
            
            if (response.Success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✓ Document uploaded successfully!");
                System.Console.ResetColor();
                
                var document = response.Document ?? response.Documents?.FirstOrDefault();
                if (document != null)
                {
                    System.Console.WriteLine($"\nDocument ID: {document.Id}");
                    System.Console.WriteLine($"File Name: {document.FileName}");
                    System.Console.WriteLine($"File Size: {document.FileSize:N0} bytes");
                    System.Console.WriteLine($"Content Type: {document.ContentType}");
                    System.Console.WriteLine($"URL: {document.Url}");
                    System.Console.WriteLine($"Created At: {document.CreatedAt}");
                }
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Upload failed: {response.Message}");
                if (response.Errors != null)
                {
                    System.Console.WriteLine($"  - {response.Errors}");
                }
                System.Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.ResetColor();
        }
    }
    
    private static async Task UploadSamplePdf(IDocumentService documentService)
    {
        System.Console.WriteLine("\n=== Create and Upload Sample PDF ===");
        
        System.Console.Write("Enter Work Order ID: ");
        if (!int.TryParse(System.Console.ReadLine(), out var workOrderId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid work order ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Document Type ID (e.g., 55): ");
        if (!int.TryParse(System.Console.ReadLine(), out var documentTypeId))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid document type ID");
            System.Console.ResetColor();
            return;
        }
        
        System.Console.Write("Enter Description: ");
        var description = System.Console.ReadLine() ?? "Sample PDF document";
        
        try
        {
            System.Console.WriteLine("\nCreating sample PDF-like content...");
            
            // Create a simple text that simulates a PDF (note: this is not a real PDF)
            // For testing purposes, we'll create a text file and name it .pdf
            // In production, you would use a PDF library to create actual PDF content
            var content = $@"%PDF-1.4
Test Document for Work Order {workOrderId}
Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Description: {description}

This is a test document upload.
Work Order Details:
- ID: {workOrderId}
- Document Type: {documentTypeId}
- Date: {DateTime.Now}

Additional Information:
This document was created as a test for the Fexa API document upload functionality.
%%EOF";
            
            var bytes = Encoding.UTF8.GetBytes(content);
            var fileName = $"work_order_{workOrderId}_doc_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            System.Console.WriteLine($"Uploading {fileName} ({bytes.Length} bytes)...");
            
            var response = await documentService.AddDocumentToWorkOrderAsync(
                workOrderId,
                documentTypeId,
                description,
                bytes,
                fileName,
                "application/pdf");
            
            if (response.Success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✓ Document uploaded successfully!");
                System.Console.ResetColor();
                
                var document = response.Document ?? response.Documents?.FirstOrDefault();
                if (document != null)
                {
                    System.Console.WriteLine($"\nDocument ID: {document.Id}");
                    System.Console.WriteLine($"File Name: {document.FileName ?? document.Filename}");
                    System.Console.WriteLine($"File Size: {document.FileSize} bytes");
                    System.Console.WriteLine($"Content Type: {document.ContentType}");
                    System.Console.WriteLine($"URL: {document.Url}");
                    System.Console.WriteLine($"Created At: {document.CreatedAt}");
                }
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Upload failed: {response.Message}");
                if (response.Errors != null)
                {
                    System.Console.WriteLine($"  - {response.Errors}");
                }
                System.Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.ResetColor();
        }
    }
}