using System.Text.Json;
using System.Web;

namespace Fexa.ApiClient.Models;

public class FilterBuilder
{
    private readonly List<FexaFilter> _filters = new();
    
    public FilterBuilder Where(string property, object value)
    {
        _filters.Add(new FexaFilter(property, value));
        return this;
    }
    
    public FilterBuilder WhereIn(string property, params object[] values)
    {
        _filters.Add(new FexaFilter(property, values, FilterOperators.In));
        return this;
    }
    
    public FilterBuilder WhereNotIn(string property, params object[] values)
    {
        _filters.Add(new FexaFilter(property, values, FilterOperators.NotIn));
        return this;
    }
    
    public FilterBuilder WhereBetween(string property, object startValue, object endValue)
    {
        _filters.Add(new FexaFilter(property, new[] { startValue, endValue }, FilterOperators.Between));
        return this;
    }
    
    public FilterBuilder WhereDateBetween(string property, DateTime startDate, DateTime endDate)
    {
        return WhereBetween(property, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
    }
    
    public FilterBuilder AddFilter(FexaFilter filter)
    {
        _filters.Add(filter);
        return this;
    }
    
    public FilterBuilder AddFilters(IEnumerable<FexaFilter> filters)
    {
        if (filters != null)
        {
            _filters.AddRange(filters);
        }
        return this;
    }
    
    public List<FexaFilter> Build()
    {
        return _filters.ToList();
    }
    
    public string ToJson()
    {
        if (!_filters.Any())
            return "[]";
            
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        return JsonSerializer.Serialize(_filters, options);
    }
    
    public string ToUrlEncoded()
    {
        return HttpUtility.UrlEncode(ToJson());
    }
    
    public static FilterBuilder Create()
    {
        return new FilterBuilder();
    }
    
    // Convenience methods for common filters
    public FilterBuilder WhereWorkOrderId(int workOrderId)
    {
        return Where("workorders.id", workOrderId);
    }
    
    public FilterBuilder WhereWorkOrderIds(params int[] workOrderIds)
    {
        return WhereIn("workorders.id", workOrderIds.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereVendorId(int vendorId)
    {
        return Where("vendors.id", vendorId);
    }
    
    public FilterBuilder WhereVendorIds(params int[] vendorIds)
    {
        return WhereIn("vendors.id", vendorIds.Cast<object>().ToArray());
    }
    
    // Visit-specific filter methods
    public FilterBuilder WhereVisitId(int visitId)
    {
        return Where("visits.id", visitId);
    }
    
    public FilterBuilder WhereVisitIds(params int[] visitIds)
    {
        return WhereIn("visits.id", visitIds.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereVisitStatus(string status)
    {
        return Where("visits.status", status);
    }
    
    public FilterBuilder WhereVisitStatuses(params string[] statuses)
    {
        return WhereIn("visits.status", statuses.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereTechnicianId(int technicianId)
    {
        return Where("technicians.id", technicianId);
    }
    
    public FilterBuilder WhereTechnicianIds(params int[] technicianIds)
    {
        return WhereIn("technicians.id", technicianIds.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereClientId(int clientId)
    {
        return Where("clients.id", clientId);
    }
    
    public FilterBuilder WhereClientIds(params int[] clientIds)
    {
        return WhereIn("clients.id", clientIds.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereLocationId(int locationId)
    {
        return Where("locations.id", locationId);
    }
    
    public FilterBuilder WhereLocationIds(params int[] locationIds)
    {
        return WhereIn("locations.id", locationIds.Cast<object>().ToArray());
    }
    
    public FilterBuilder WhereScheduledDateBetween(DateTime startDate, DateTime endDate)
    {
        return WhereDateBetween("visits.scheduled_date", startDate, endDate);
    }
    
    public FilterBuilder WhereActualDateBetween(DateTime startDate, DateTime endDate)
    {
        return WhereDateBetween("visits.actual_date", startDate, endDate);
    }
    
    public FilterBuilder WhereCompletedDateBetween(DateTime startDate, DateTime endDate)
    {
        return WhereDateBetween("visits.completed_at", startDate, endDate);
    }
    
    // Single date filter methods for visits
    public FilterBuilder WhereScheduledDate(DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        return WhereBetween("visits.scheduled_date", dateStr, dateStr);
    }
    
    public FilterBuilder WhereActualDate(DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        return WhereBetween("visits.actual_date", dateStr, dateStr);
    }
    
    public FilterBuilder WhereCompletedDate(DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        return WhereBetween("visits.completed_at", dateStr, dateStr);
    }
    
    public FilterBuilder WhereScheduledAfter(DateTime date)
    {
        return WhereBetween("visits.scheduled_date", date.ToString("yyyy-MM-dd"), "2099-12-31");
    }
    
    public FilterBuilder WhereScheduledBefore(DateTime date)
    {
        return WhereBetween("visits.scheduled_date", "1900-01-01", date.ToString("yyyy-MM-dd"));
    }
    
    public FilterBuilder WhereActualAfter(DateTime date)
    {
        return WhereBetween("visits.actual_date", date.ToString("yyyy-MM-dd"), "2099-12-31");
    }
    
    public FilterBuilder WhereActualBefore(DateTime date)
    {
        return WhereBetween("visits.actual_date", "1900-01-01", date.ToString("yyyy-MM-dd"));
    }
    
    // Generic date filters that can be used with any date field
    public FilterBuilder WhereDate(string property, DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        return WhereBetween(property, dateStr, dateStr);
    }
    
    public FilterBuilder WhereDateAfter(string property, DateTime date)
    {
        return WhereBetween(property, date.ToString("yyyy-MM-dd"), "2099-12-31");
    }
    
    public FilterBuilder WhereDateBefore(string property, DateTime date)
    {
        return WhereBetween(property, "1900-01-01", date.ToString("yyyy-MM-dd"));
    }
}