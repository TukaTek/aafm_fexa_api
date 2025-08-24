namespace Fexa.ApiClient.Models;

// Example DTOs - Replace with actual Fexa API models
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QueryParameters
{
    // Fexa uses start/limit instead of page/pageSize
    public int Start { get; set; } = 0;
    public int Limit { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? Search { get; set; }
    public List<FexaFilter>? Filters { get; set; }
    
    // Helper properties for pagination
    public int Page 
    { 
        get => (Start / Limit) + 1; 
        set => Start = (value - 1) * Limit; 
    }
    
    public int PageSize 
    { 
        get => Limit; 
        set => Limit = value; 
    }
    
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>
        {
            ["start"] = Start.ToString(),
            ["limit"] = Limit.ToString()
        };
        
        if (!string.IsNullOrWhiteSpace(SortBy))
            dict["sortBy"] = SortBy;
            
        if (SortDescending)
            dict["sortDesc"] = "true";
            
        if (!string.IsNullOrWhiteSpace(Search))
            dict["search"] = Search;
            
        if (Filters?.Any() == true)
        {
            var filterBuilder = FilterBuilder.Create();
            foreach (var filter in Filters)
            {
                filterBuilder.AddFilter(filter);
            }
            // Filters should NOT be URL encoded here - they will be encoded by Uri.EscapeDataString in BuildQueryString
            dict["filters"] = filterBuilder.ToJson();
        }
            
        return dict;
    }
    
    public static QueryParameters Create()
    {
        return new QueryParameters();
    }
    
    public QueryParameters WithFilters(Action<FilterBuilder> configureFilters)
    {
        var builder = FilterBuilder.Create();
        configureFilters(builder);
        Filters = builder.Build();
        return this;
    }
}