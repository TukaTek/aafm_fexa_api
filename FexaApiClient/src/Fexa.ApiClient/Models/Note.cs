using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Note
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("note")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("note_type")]
    public NoteType? NoteType { get; set; }
    
    [JsonPropertyName("note_type_id")]
    public int? NoteTypeId { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("notable_id")]
    public int? NotableId { get; set; }
    
    [JsonPropertyName("notable_type")]
    public string? NotableType { get; set; }
    
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
    
    [JsonPropertyName("action_required")]
    public bool? ActionRequired { get; set; }
    
    [JsonPropertyName("is_external_note")]
    public bool? IsExternalNote { get; set; }
    
    [JsonPropertyName("active")]
    public bool? Active { get; set; }
    
    [JsonPropertyName("creator")]
    public Creator? Creator { get; set; }
    
    // Legacy mappings for backward compatibility
    [JsonIgnore]
    public string? ObjectType => NotableType;
    
    [JsonIgnore]
    public int? ObjectId => NotableId;
    
    [JsonIgnore]
    public bool? IsPrivate => Visibility == "private";
    
    [JsonIgnore]
    public bool? IsInternal => Visibility == "internal";
    
    [JsonIgnore]
    public NoteUser? User => Creator != null ? new NoteUser 
    { 
        Id = Creator.Id, 
        Email = Creator.Email,
        FirstName = Creator.Organization?.Name,
        LastName = ""
    } : null;
}

public class NoteUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class NoteType
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class Creator
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("organization")]
    public NoteOrganization? Organization { get; set; }
}

public class NoteOrganization
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

