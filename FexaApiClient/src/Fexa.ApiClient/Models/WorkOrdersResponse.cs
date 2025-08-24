using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkOrdersResponse
{
    [JsonPropertyName("workorders")]
    public List<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class SingleWorkOrderResponse
{
    [JsonPropertyName("workorders")]
    public WorkOrder? WorkOrder { get; set; }
}