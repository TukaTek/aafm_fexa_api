using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Document
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("document_type_id")]
    public int DocumentTypeId { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }
    
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
    
    [JsonPropertyName("file_size")]
    public long? FileSize { get; set; }
    
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
}

public class DocumentUploadRequest
{
    public int WorkOrderId { get; set; }
    public int DocumentTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Stream? FileStream { get; set; }
    public byte[]? FileBytes { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
}

public class DocumentUploadResponse
{
    [JsonPropertyName("document")]
    public Document? Document { get; set; }
    
    [JsonPropertyName("documents")]
    public List<Document>? Documents { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("errors")]
    public object? Errors { get; set; }
}

// DTO for simplified Web API responses
public class DocumentDto
{
    public int Id { get; set; }
    public int DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? Url { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
}