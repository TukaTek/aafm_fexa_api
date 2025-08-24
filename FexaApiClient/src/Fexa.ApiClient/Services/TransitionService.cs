using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Linq;

namespace Fexa.ApiClient.Services;

public class TransitionService : ITransitionService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<TransitionService> _logger;
    private const string TransitionsEndpoint = "/api/ev1/users/list_transitions";
    
    // Cache ALL transitions for the session since they don't change often
    private List<WorkflowTransition>? _cachedAllTransitions;
    private DateTime? _cacheTime;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15); // Extended cache time since data doesn't change
    
    public TransitionService(IFexaApiService apiService, ILogger<TransitionService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Ensures all transitions are loaded into cache. Call this before any other operations.
    /// </summary>
    private async Task<List<WorkflowTransition>> EnsureTransitionsLoadedAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cachedAllTransitions != null && _cacheTime.HasValue && 
            DateTime.UtcNow - _cacheTime.Value < _cacheExpiry)
        {
            _logger.LogDebug("Using cached transitions (cached at {CacheTime}, {Count} transitions)", 
                _cacheTime, _cachedAllTransitions.Count);
            return _cachedAllTransitions;
        }
        
        _logger.LogInformation("Cache expired or empty, fetching ALL workflow transitions from API...");
        
        var allTransitions = new List<WorkflowTransition>();
        var pageSize = 100;
        var currentPage = 0;
        var hasMoreData = true;
        var totalAvailable = 0;
        
        while (hasMoreData)
        {
            var start = currentPage * pageSize;
            
            try
            {
                var endpoint = $"{TransitionsEndpoint}?start={start}&limit={pageSize}";
                var response = await _apiService.GetAsync<TransitionsResponse>(endpoint, cancellationToken);
                
                if (response == null || !response.Success)
                {
                    _logger.LogWarning("Failed to fetch transitions page {Page}", currentPage + 1);
                    hasMoreData = false;
                    continue;
                }
                
                if (currentPage == 0)
                {
                    totalAvailable = response.TotalCount;
                    _logger.LogInformation("Total transitions available: {Total}", totalAvailable);
                }
                
                if (response.Transitions != null && response.Transitions.Any())
                {
                    allTransitions.AddRange(response.Transitions);
                    _logger.LogDebug("Fetched page {Page} with {Count} transitions. Total so far: {Total}", 
                        currentPage + 1, response.Transitions.Count, allTransitions.Count);
                    
                    // Check if there are more pages
                    hasMoreData = response.Transitions.Count == pageSize && 
                                 allTransitions.Count < totalAvailable;
                }
                else
                {
                    hasMoreData = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transitions page {Page}", currentPage + 1);
                hasMoreData = false;
            }
            
            currentPage++;
            
            // Safety check to prevent infinite loops
            if (currentPage > 20) // Max 2000 transitions
            {
                _logger.LogWarning("Reached maximum page limit, stopping pagination");
                break;
            }
        }
        
        _logger.LogInformation("Successfully loaded {Count} transitions into cache (expected {Expected})", 
            allTransitions.Count, totalAvailable);
        
        // Cache the results
        _cachedAllTransitions = allTransitions;
        _cacheTime = DateTime.UtcNow;
        
        // Log Work Order specific stats
        var workOrderTransitions = allTransitions.Where(t => t.WorkflowObjectType == "Assignment").ToList();
        var workOrderStatuses = workOrderTransitions
            .SelectMany(t => new[] { t.FromStatus, t.ToStatus })
            .Where(s => s != null)
            .GroupBy(s => s!.Id)
            .Select(g => g.First())
            .OrderBy(s => s!.Name)
            .ToList();
            
        _logger.LogInformation("Work Order (Assignment) statistics: {TransitionCount} transitions, {StatusCount} unique statuses",
            workOrderTransitions.Count, workOrderStatuses.Count);
        
        return allTransitions;
    }
    
    public async Task<TransitionsResponse> GetTransitionsAsync(int start = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching workflow transitions page from API (start: {Start}, limit: {Limit})", start, limit);
        
        try
        {
            var endpoint = $"{TransitionsEndpoint}?start={start}&limit={limit}";
            var response = await _apiService.GetAsync<TransitionsResponse>(endpoint, cancellationToken);
            
            if (response == null)
            {
                _logger.LogWarning("Received null response from transitions endpoint");
                return new TransitionsResponse { Success = false };
            }
            
            _logger.LogInformation("Successfully fetched {Count} transitions (total available: {Total})", 
                response.Transitions?.Count ?? 0, response.TotalCount);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch transitions");
            throw;
        }
    }
    
    public async Task<List<WorkflowTransition>> GetAllTransitionsAsync(int maxPages = 10, CancellationToken cancellationToken = default)
    {
        // Simply use the cache-first approach
        return await EnsureTransitionsLoadedAsync(cancellationToken);
    }
    
    public async Task<List<WorkflowStatus>> GetUniqueStatusesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting unique statuses from cached transitions");
        
        // Ensure all transitions are loaded
        var allTransitions = await EnsureTransitionsLoadedAsync(cancellationToken);
        
        if (!allTransitions.Any())
        {
            return new List<WorkflowStatus>();
        }
        
        // Focus on Work Order (Assignment) statuses first
        var workOrderTransitions = allTransitions.Where(t => t.WorkflowObjectType == "Assignment").ToList();
        
        // Extract unique Work Order statuses
        var workOrderStatuses = new Dictionary<int, WorkflowStatus>();
        
        foreach (var transition in workOrderTransitions)
        {
            if (transition.FromStatus != null && !workOrderStatuses.ContainsKey(transition.FromStatusId))
            {
                workOrderStatuses[transition.FromStatusId] = transition.FromStatus;
            }
            if (transition.ToStatus != null && !workOrderStatuses.ContainsKey(transition.ToStatusId))
            {
                workOrderStatuses[transition.ToStatusId] = transition.ToStatus;
            }
        }
        
        var uniqueStatuses = workOrderStatuses.Values
            .OrderBy(s => s.Name)
            .ToList();
        
        _logger.LogInformation("Found {Count} unique Work Order (Assignment) statuses", uniqueStatuses.Count);
        
        // Log the most common Work Order statuses
        var commonStatuses = uniqueStatuses.Where(s => 
            s.Name.Contains("New") || 
            s.Name.Contains("Action Required") || 
            s.Name.Contains("In Progress") || 
            s.Name.Contains("Scheduled") ||
            s.Name.Contains("Completed") ||
            s.Name.Contains("Cancelled")).ToList();
            
        if (commonStatuses.Any())
        {
            _logger.LogInformation("Common Work Order statuses found: {Statuses}", 
                string.Join(", ", commonStatuses.Select(s => $"{s.Name} (ID: {s.Id})")));
        }
        
        return uniqueStatuses;
    }
    
    public async Task<List<WorkflowTransition>> GetTransitionsByTypeAsync(string workflowObjectType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting transitions for workflow object type: {Type} from cache", workflowObjectType);
        
        // Ensure all transitions are loaded
        var allTransitions = await EnsureTransitionsLoadedAsync(cancellationToken);
        
        if (!allTransitions.Any())
        {
            return new List<WorkflowTransition>();
        }
        
        var filtered = allTransitions
            .Where(t => t.WorkflowObjectType.Equals(workflowObjectType, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        _logger.LogInformation("Found {Count} transitions for type '{Type}' (from cache)", filtered.Count, workflowObjectType);
        
        // If this is for Work Orders, log some useful info
        if (workflowObjectType.Equals("Assignment", StringComparison.OrdinalIgnoreCase))
        {
            var uniqueStatuses = filtered
                .SelectMany(t => new[] { t.FromStatus, t.ToStatus })
                .Where(s => s != null)
                .GroupBy(s => s!.Id)
                .Count();
            _logger.LogInformation("Work Order transitions use {Count} unique statuses", uniqueStatuses);
        }
        
        return filtered;
    }
    
    public async Task<List<WorkflowTransition>> GetTransitionsFromStatusAsync(int statusId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting transitions from status ID: {StatusId} from cache", statusId);
        
        // Ensure all transitions are loaded
        var allTransitions = await EnsureTransitionsLoadedAsync(cancellationToken);
        
        if (!allTransitions.Any())
        {
            return new List<WorkflowTransition>();
        }
        
        // Focus on Work Order transitions first
        var workOrderTransitions = allTransitions
            .Where(t => t.WorkflowObjectType == "Assignment" && t.FromStatusId == statusId)
            .ToList();
        
        var allFiltered = allTransitions
            .Where(t => t.FromStatusId == statusId)
            .ToList();
        
        _logger.LogInformation("Found {Count} transitions from status ID {StatusId} ({WorkOrderCount} are Work Order transitions)", 
            allFiltered.Count, statusId, workOrderTransitions.Count);
        
        // Log the status name if we have it
        var statusName = allFiltered.FirstOrDefault()?.FromStatus?.Name;
        if (!string.IsNullOrEmpty(statusName))
        {
            _logger.LogInformation("Status name: '{StatusName}'", statusName);
        }
        
        return allFiltered;
    }
    
    public async Task<List<WorkflowTransition>> GetTransitionsToStatusAsync(int statusId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting transitions to status ID: {StatusId} from cache", statusId);
        
        // Ensure all transitions are loaded
        var allTransitions = await EnsureTransitionsLoadedAsync(cancellationToken);
        
        if (!allTransitions.Any())
        {
            return new List<WorkflowTransition>();
        }
        
        // Focus on Work Order transitions first
        var workOrderTransitions = allTransitions
            .Where(t => t.WorkflowObjectType == "Assignment" && t.ToStatusId == statusId)
            .ToList();
        
        var allFiltered = allTransitions
            .Where(t => t.ToStatusId == statusId)
            .ToList();
        
        _logger.LogInformation("Found {Count} transitions to status ID {StatusId} ({WorkOrderCount} are Work Order transitions)", 
            allFiltered.Count, statusId, workOrderTransitions.Count);
        
        // Log the status name if we have it
        var statusName = allFiltered.FirstOrDefault()?.ToStatus?.Name;
        if (!string.IsNullOrEmpty(statusName))
        {
            _logger.LogInformation("Status name: '{StatusName}'", statusName);
        }
        
        return allFiltered;
    }
    
    public async Task<List<WorkflowStatus>> GetWorkOrderStatusesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting Work Order statuses from cache");
        
        // Ensure all transitions are loaded
        var allTransitions = await EnsureTransitionsLoadedAsync(cancellationToken);
        
        if (!allTransitions.Any())
        {
            _logger.LogWarning("No transitions found in cache");
            return new List<WorkflowStatus>();
        }
        
        // Filter to only Work Order transitions
        var workOrderTransitions = allTransitions
            .Where(t => t.WorkflowObjectType == "Work Order")
            .ToList();
        
        _logger.LogInformation("Processing {Count} Work Order transitions", workOrderTransitions.Count);
        
        // Extract unique statuses
        var statusMap = new Dictionary<int, WorkflowStatus>();
        
        foreach (var transition in workOrderTransitions)
        {
            if (transition.FromStatus != null && !statusMap.ContainsKey(transition.FromStatusId))
            {
                statusMap[transition.FromStatusId] = transition.FromStatus;
            }
            if (transition.ToStatus != null && !statusMap.ContainsKey(transition.ToStatusId))
            {
                statusMap[transition.ToStatusId] = transition.ToStatus;
            }
        }
        
        var workOrderStatuses = statusMap.Values
            .OrderBy(s => s.Name)
            .ToList();
        
        _logger.LogInformation("Found {Count} unique Work Order statuses", workOrderStatuses.Count);
        
        // Log all Work Order statuses for reference
        foreach (var status in workOrderStatuses)
        {
            _logger.LogDebug("Work Order Status: '{Name}' (ID: {Id})", status.Name, status.Id);
        }
        
        // Identify and log the most commonly used statuses
        var commonStatusNames = new[] { "New", "Action Required", "In Progress", "Scheduled", "Completed", "Cancelled" };
        var commonStatuses = workOrderStatuses
            .Where(s => commonStatusNames.Any(name => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        
        if (commonStatuses.Any())
        {
            _logger.LogInformation("Common Work Order statuses available:");
            foreach (var status in commonStatuses)
            {
                _logger.LogInformation("  - {Name} (ID: {Id})", status.Name, status.Id);
            }
        }
        
        return workOrderStatuses;
    }
}