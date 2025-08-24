using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class FexaFilter
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
    
    [JsonPropertyName("operator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Operator { get; set; }
    
    public FexaFilter()
    {
    }
    
    public FexaFilter(string property, object value)
    {
        Property = property;
        Value = value;
    }
    
    public FexaFilter(string property, object value, string operatorType)
    {
        Property = property;
        Value = value;
        Operator = operatorType;
    }
}

public static class FilterOperators
{
    public const string In = "in";
    public const string NotIn = "not in";
    public const string Between = "between";
}