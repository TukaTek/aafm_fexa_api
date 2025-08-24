using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkflowTransition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("from_status_id")]
    public int FromStatusId { get; set; }
    
    [JsonPropertyName("to_status_id")]
    public int ToStatusId { get; set; }
    
    [JsonPropertyName("workflow_object_type")]
    public string WorkflowObjectType { get; set; } = string.Empty;
    
    [JsonPropertyName("from_status")]
    public WorkflowStatus? FromStatus { get; set; }
    
    [JsonPropertyName("to_status")]
    public WorkflowStatus? ToStatus { get; set; }
}

public class WorkflowStatus
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TransitionsResponse
{
    [JsonPropertyName("transitions")]
    public List<WorkflowTransition> Transitions { get; set; } = new();
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}