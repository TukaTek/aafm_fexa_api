# Fexa API Documentation - AAFM Integration

**Repository**: [https://github.com/TukaTek/aafm_fexa_api](https://github.com/TukaTek/aafm_fexa_api)  
**Organization**: [TukaTek](https://github.com/TukaTek)  
**Status**: Active Development (December 2024)

## Table of Contents
1. [Overview](#overview)
2. [Data Model Relationships](#data-model-relationships)
3. [API Client Library Structure](#api-client-library-structure)
4. [Authentication](#authentication)
5. [Service Endpoints](#service-endpoints)
6. [Workflow System](#workflow-system)
7. [Implementation Examples](#implementation-examples)

---

## Overview

The Fexa API provides OAuth 2.0 authenticated access to AAFM (American Association of Fleet Managers) facility management data, including work orders, assignments, visits, users, invoices, and workflow transitions.

### Key Features
- RESTful API with JSON responses
- OAuth 2.0 Client Credentials authentication
- Automatic token refresh
- Comprehensive error handling
- Rate limiting with retry logic
- Circuit breaker pattern for resilience

### Base URLs
- **Sandbox**: `https://aafmapisandbox.fexa.io/`
- **Production**: Contact AAFM for production URL
- **API Documentation**: `https://aafmapisandbox.fexa.io/fexa_docs/index.html`

---

## Data Model Relationships

### Core Entity Hierarchy

```
Work Order
    ├── Assignments (Multiple subcontractors)
    │   ├── Subcontractor (via role_id)
    │   ├── NTE Amount
    │   └── Visits (Scheduled work sessions)
    │       ├── Check-in/Check-out times
    │       ├── Work performed
    │       └── Signatures
    ├── Facilities (Locations)
    ├── Client Invoices
    └── Workflow Status
```

### 1. Work Order Structure

A work order represents a maintenance request or scheduled work at a facility.

#### Key Fields
```json
{
  "workorders": {
    "id": 181198,
    "description": "Work description",
    "priority_id": 14,
    "category_id": 293,
    "placed_by": 3,                    // User who created the WO
    "placed_for": 454161,               // Client ID
    "assigned_to": 432083,              // Primary assignee
    "facility_id": 531600,              // Location
    "next_visit": "2025-08-12T07:00:00.000Z",
    "completed_visits": 0,
    "upcoming_visits": 1,
    "assignments": [...],               // Array of subcontractor assignments
    "visits": [...],                    // Array of scheduled visits
    "workorder_facilities": [...],      // Associated facilities
    "object_state": {                   // Current workflow status
      "status": {
        "id": 236,
        "name": "Pending Signoffs / Photos"
      }
    }
  }
}
```

#### Financial Fields
- `client_not_to_exceed`: Client's maximum approved amount
- `client_balance`, `client_sub_total`, `client_tax`, `client_total`
- `assignment_balance`, `assignment_sub_total`, `assignment_tax`, `assignment_total`
- Various product totals (labor, material, travel, tax)

### 2. Assignment Structure

An assignment links a work order to a specific subcontractor with defined scope and terms.

#### Key Fields
```json
{
  "assignments": {
    "id": 1213567,
    "workorder_id": 180901,
    "role_id": 460538,                  // Subcontractor identifier
    "category_id": 315,                 // Type of work
    "facility_id": 521897,
    "priority_id": 13,
    "initial_response_deadline": "2025-08-13T00:00:00.000Z",
    "initial_arrival_deadline": "2025-08-14T00:15:00.000Z",
    "completion_deadline": "2025-08-15T00:30:00.000Z",
    "date_accepted": "2025-08-12T20:27:14.320Z",
    "date_completed": null,
    "subcontractor_not_to_exceed": {
      "amount": "185.0"                 // NTE amount for this subcontractor
    },
    "role": {                           // Subcontractor details
      "id": 460538,
      "type": "Roles::EntityRole::SubcontractorRole",
      "entity": {
        "id": 300756,
        "type": "Entities::Organization",
        "default_dispatch_address": {
          "company": "All Florida - Xavier Cardona",
          "address1": "710 International Parkway",
          "city": "Sunrise",
          "state": "FL",
          "postal_code": "33325"
        }
      }
    }
  }
}
```

#### Assignment Status Tracking
- `past_initial_response_deadline`: Boolean
- `past_initial_arrival_deadline`: Boolean
- `past_completion_deadline`: Boolean
- `object_state`: Current workflow status

#### Invoice Tracking
- `pending_invoice_total`, `approved_invoice_total`
- `submitted_pending_invoice_total`
- `combined_invoice_total`
- Separate tracking for sub-totals and tax

### 3. Visit Structure

A visit represents a scheduled work session for an assignment.

#### Key Fields
```json
{
  "visits": {
    "id": 225283,
    "assignment_id": 1213567,           // Links to assignment
    "workorder_id": 180901,             // Direct link to work order
    "start_date": "2025-08-14T00:15:00.000Z",
    "end_date": "2025-08-14T01:15:00.000Z",
    "check_in_time": null,
    "check_out_time": null,
    "facility_id": 521897,
    "category_id": 315,
    "work_performed": null,
    "signature_required": true,
    "signature_accepted": false,
    "technician_count": 1,
    "object_state": {                   // Visit status
      "status": {
        "id": 148,
        "name": "Schedule Pending Acceptance"
      }
    },
    "assignment": {                     // Embedded assignment with subcontractor
      "role": {
        "entity": {
          "default_dispatch_address": {
            "company": "Subcontractor Name"
          }
        }
      }
    }
  }
}
```

#### Visit Tracking
- `check_in_within_radius`: Geolocation validation
- `check_in_coordinates`, `check_out_coordinates`
- `store_signature_document_id`, `technician_signature_document_id`
- `signature_refused`, `signature_unavailable`

### 4. Relationship Mappings

#### Work Order → Assignments (1:Many)
- One work order can have multiple assignments
- Each assignment is for a different subcontractor
- Each has its own NTE amount and deadlines

#### Assignment → Subcontractor (1:1)
- Each assignment links to exactly one subcontractor via `role_id`
- Subcontractor details stored in `role.entity`

#### Assignment → Visits (1:Many)
- Each assignment can have multiple scheduled visits
- Visits inherit subcontractor from parent assignment

#### Visit → Assignment → Subcontractor
- Each visit belongs to one assignment
- Inherits subcontractor information through assignment

---

## API Client Library Structure

### Solution Architecture

```
FexaApiClient/
├── src/
│   ├── Fexa.ApiClient/                 # Core library
│   │   ├── Configuration/              # Settings and options
│   │   ├── Exceptions/                 # Custom exceptions
│   │   ├── Filters/                    # Query filters
│   │   ├── Handlers/                   # HTTP handlers
│   │   ├── Models/                     # Data models
│   │   └── Services/                   # Service implementations
│   └── Fexa.ApiClient.Console/         # Test console app
├── tests/
│   └── Fexa.ApiClient.Tests/          # Unit tests
└── FexaApiClient.sln
```

### Core Components

#### 1. Configuration System
```csharp
public class FexaApiOptions
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public int TokenRefreshBufferSeconds { get; set; } = 300;
    public int MaxRetryAttempts { get; set; } = 3;
}
```

#### 2. Service Layer

##### IFexaApiService
Core HTTP operations with retry and circuit breaker policies:
```csharp
public interface IFexaApiService
{
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);
    Task<T> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default);
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
}
```

##### ITokenService
OAuth 2.0 token management:
```csharp
public interface ITokenService
{
    Task<TokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default);
    void ClearCache();
}
```

##### Domain Services
- `IWorkOrderService`: Work order operations
- `IVisitService`: Visit management
- `IUserService`: User operations
- `IClientInvoiceService`: Invoice management
- `ITransitionService`: Workflow transitions

#### 3. Filter System

The `FilterBuilder` provides fluent API for building complex queries:

```csharp
var filter = new FilterBuilder()
    .AddFilter("status", "active")
    .AddInFilter("category_id", new[] { 130, 315 })
    .AddDateRangeFilter("created_at", startDate, endDate)
    .AddBetweenFilter("amount", 100, 500)
    .Build();
```

Supported operators:
- `equals`: Exact match
- `in`: Multiple values
- `not_in`: Exclusion
- `between`: Range (numeric or date)
- `greater_than`, `less_than`
- `contains`: Partial match

#### 4. Exception Hierarchy

```csharp
FexaApiException                    // Base exception
├── FexaAuthenticationException     // Auth failures
├── FexaRateLimitException         // Rate limiting
├── FexaValidationException        // Validation errors
└── FexaNotFoundException          // Resource not found
```

---

## Authentication

### OAuth 2.0 Client Credentials Flow

1. **Token Request**
```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&
client_id={CLIENT_ID}&
client_secret={CLIENT_SECRET}
```

2. **Token Response**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 7200,
  "created_at": 1628789123
}
```

3. **Using the Token**
```http
GET /api/v2/workorders
Authorization: Bearer {access_token}
Accept: application/json
```

### Automatic Token Refresh
The client library automatically:
- Caches tokens until expiration
- Refreshes tokens before expiry (5-minute buffer)
- Retries on 401 responses with new token

---

## Service Endpoints

### Work Order Service

#### Get Work Orders (Paginated)
```csharp
var parameters = new QueryParameters
{
    Start = 0,
    Limit = 100,
    Filters = filterJson
};
var response = await workOrderService.GetWorkOrdersAsync(parameters);
```

#### Get Single Work Order
```csharp
var workOrder = await workOrderService.GetWorkOrderAsync(workOrderId);
```

#### Get All Work Orders (Multiple Pages)
```csharp
var allWorkOrders = await workOrderService.GetAllWorkOrdersAsync(
    parameters: null, 
    maxPages: 10
);
```

#### Filter by Status
```csharp
var response = await workOrderService.GetWorkOrdersByStatusAsync("In Progress");
```

#### Filter by Vendor
```csharp
var response = await workOrderService.GetWorkOrdersByVendorAsync(vendorId);
```

#### Filter by Date Range
```csharp
var response = await workOrderService.GetWorkOrdersByDateRangeAsync(
    startDate, 
    endDate
);
```

### Visit Service

#### Endpoint Specifics
- Uses `/api/ev1/visits` (not v2)
- Requires filters as URL-encoded JSON array
- No prefix needed for filter fields

#### Get Visits
```csharp
var parameters = new QueryParameters
{
    Start = 0,
    Limit = 50
};
var response = await visitService.GetVisitsAsync(parameters);
```

#### Get Visits by Work Order
```csharp
var response = await visitService.GetVisitsByWorkOrderAsync(workOrderId);
```

#### Get Visits by Date Range
```csharp
// For single day, use same date for start and end
var response = await visitService.GetVisitsByDateRangeAsync(
    startDate, 
    endDate
);
```

#### Visit Filter Format
```csharp
var filter = new[]
{
    new Dictionary<string, object>
    {
        ["property"] = "start_date",
        ["operator"] = "between",
        ["value"] = new[] { 
            "2025-08-14 00:00:00", 
            "2025-08-15 23:59:59" 
        }
    }
};
```

### Transition Service

#### Get All Transitions
```csharp
var allTransitions = await transitionService.GetAllTransitionsAsync();
```

#### Get Transitions Page
```csharp
var response = await transitionService.GetTransitionsAsync(
    start: 0, 
    limit: 100
);
```

#### Get Work Order Statuses
```csharp
var statuses = await transitionService.GetWorkOrderStatusesAsync();
```

#### Get Transitions From/To Status
```csharp
var fromTransitions = await transitionService.GetTransitionsFromStatusAsync(statusId);
var toTransitions = await transitionService.GetTransitionsToStatusAsync(statusId);
```

### User Service

#### Get Users
```csharp
var response = await userService.GetUsersAsync(
    start: 0, 
    limit: 50
);
```

#### Get User by ID
```csharp
var user = await userService.GetUserAsync(userId);
```

### Client Invoice Service

#### Get Invoices
```csharp
var response = await clientInvoiceService.GetInvoicesAsync(
    start: 0, 
    limit: 50
);
```

#### Get Invoice by ID
```csharp
var invoice = await clientInvoiceService.GetInvoiceAsync(invoiceId);
```

---

## Workflow System

### Workflow Object Types

1. **Assignment** - Work order assignments to subcontractors
2. **Client Invoice** - Customer billing
3. **Subcontractor Invoice** - Vendor billing
4. **Visit** - Scheduled work sessions

### Workflow Types

Each status belongs to a workflow type indicating the general state:

1. **New** - Initial states
2. **Accepted** - Work accepted/in progress
3. **Complete** - Work completed
4. **Cancelled** - Work cancelled
5. **Declined** - Work declined

### Common Work Order Statuses

#### New States
- New
- Pending Assignment
- Assigned to Vendor

#### In Progress States
- In Progress
- Action Required
- Response Deadline Missed
- Arrival Deadline Missed

#### Completion States
- Completed
- Pending Signoffs / Photos
- Pending Client Approval
- Approved

#### Cancellation States
- Cancelled
- Declined by Vendor

### Status Transitions

Transitions define allowed status changes:

```json
{
  "id": 1234,
  "name": "Accept Work",
  "from_status_id": 17,
  "from_status": {
    "name": "Assigned to Vendor"
  },
  "to_status_id": 11,
  "to_status": {
    "name": "Accepted"
  },
  "workflow_object_type": "Assignment"
}
```

---

## Implementation Examples

### 1. Service Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddFexaApiClient(options =>
{
    options.BaseUrl = "https://aafmapisandbox.fexa.io/";
    options.ClientId = configuration["FexaApi:ClientId"];
    options.ClientSecret = configuration["FexaApi:ClientSecret"];
});
```

### 2. Get Work Orders with Filters

```csharp
public async Task<List<WorkOrder>> GetActiveWorkOrders()
{
    var filter = new FilterBuilder()
        .AddFilter("workorders.status", "In Progress")
        .AddDateRangeFilter("workorders.created_at", 
            DateTime.UtcNow.AddDays(-30), 
            DateTime.UtcNow)
        .Build();

    var parameters = new QueryParameters
    {
        Start = 0,
        Limit = 100,
        Filters = filter
    };

    var response = await _workOrderService.GetWorkOrdersAsync(parameters);
    return response.Data?.ToList() ?? new List<WorkOrder>();
}
```

### 3. Get Subcontractor Assignments

```csharp
public async Task<List<Assignment>> GetSubcontractorAssignments(int workOrderId)
{
    var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);
    
    var assignments = workOrder.Assignments?
        .Where(a => a.Role?.Type == "Roles::EntityRole::SubcontractorRole")
        .Select(a => new
        {
            AssignmentId = a.Id,
            SubcontractorId = a.RoleId,
            CompanyName = a.Role?.Entity?.DefaultDispatchAddress?.Company,
            NteAmount = a.SubcontractorNotToExceed?.Amount,
            Status = a.ObjectState?.Status?.Name
        })
        .ToList();
    
    return assignments;
}
```

### 4. Track Visit Progress

```csharp
public async Task<VisitStatus> GetVisitStatus(int visitId)
{
    var parameters = new QueryParameters
    {
        Filters = $"[{{\"property\":\"id\",\"operator\":\"equals\",\"value\":{visitId}}}]"
    };
    
    var response = await _visitService.GetVisitsAsync(parameters);
    var visit = response.Data?.FirstOrDefault();
    
    if (visit != null)
    {
        return new VisitStatus
        {
            VisitId = visit.Id,
            Status = visit.ObjectState?.Status?.Name,
            CheckedIn = visit.CheckInTime.HasValue,
            CheckedOut = visit.CheckOutTime.HasValue,
            WorkPerformed = visit.WorkPerformed,
            SignatureCollected = visit.SignatureAccepted
        };
    }
    
    return null;
}
```

### 5. Monitor Workflow Transitions

```csharp
public async Task<List<string>> GetAvailableTransitions(int statusId)
{
    var transitions = await _transitionService
        .GetTransitionsFromStatusAsync(statusId);
    
    return transitions
        .Where(t => t.WorkflowObjectType == "Assignment")
        .Select(t => t.ToStatus?.Name)
        .Distinct()
        .OrderBy(s => s)
        .ToList();
}
```

### 6. Handle Rate Limiting

```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
{
    try
    {
        return await operation();
    }
    catch (FexaRateLimitException ex)
    {
        _logger.LogWarning($"Rate limit hit. Retry after: {ex.RetryAfter} seconds");
        
        if (ex.RetryAfter.HasValue)
        {
            await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter.Value));
            return await operation();
        }
        
        throw;
    }
}
```

---

## Error Handling

### Common Error Responses

#### 401 Unauthorized
```json
{
  "error": "invalid_token",
  "error_description": "The access token is invalid"
}
```

#### 404 Not Found
```json
{
  "error": "not_found",
  "message": "Work order not found"
}
```

#### 422 Validation Error
```json
{
  "errors": {
    "filters": ["Invalid filter format"]
  }
}
```

#### 429 Rate Limited
```json
{
  "error": "rate_limit_exceeded",
  "retry_after": 60
}
```

### Exception Handling Pattern

```csharp
try
{
    var workOrder = await _workOrderService.GetWorkOrderAsync(id);
}
catch (FexaNotFoundException ex)
{
    _logger.LogError($"Work order {id} not found");
    return NotFound();
}
catch (FexaAuthenticationException ex)
{
    _logger.LogError($"Authentication failed: {ex.Message}");
    return Unauthorized();
}
catch (FexaApiException ex)
{
    _logger.LogError($"API error: {ex.Message}");
    _logger.LogError($"Response: {ex.ResponseContent}");
    return StatusCode(500);
}
```

---

## Best Practices

### 1. Use Dependency Injection
```csharp
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    
    public WorkOrderController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }
}
```

### 2. Implement Caching
```csharp
public class CachedTransitionService : ITransitionService
{
    private readonly IMemoryCache _cache;
    private readonly ITransitionService _innerService;
    
    public async Task<List<WorkflowStatus>> GetWorkOrderStatusesAsync()
    {
        return await _cache.GetOrCreateAsync(
            "workorder_statuses",
            async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                return await _innerService.GetWorkOrderStatusesAsync();
            });
    }
}
```

### 3. Use Cancellation Tokens
```csharp
public async Task<IActionResult> GetWorkOrders(CancellationToken cancellationToken)
{
    var parameters = new QueryParameters { Limit = 100 };
    var response = await _workOrderService.GetWorkOrdersAsync(
        parameters, 
        cancellationToken
    );
    return Ok(response);
}
```

### 4. Log API Interactions
```csharp
_logger.LogInformation("Fetching work order {WorkOrderId}", id);
try
{
    var result = await _workOrderService.GetWorkOrderAsync(id);
    _logger.LogInformation("Successfully retrieved work order {WorkOrderId}", id);
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to fetch work order {WorkOrderId}", id);
    throw;
}
```

### 5. Handle Pagination Properly
```csharp
public async Task<List<WorkOrder>> GetAllWorkOrders()
{
    var allWorkOrders = new List<WorkOrder>();
    var start = 0;
    const int limit = 100;
    
    while (true)
    {
        var response = await _workOrderService.GetWorkOrdersAsync(
            new QueryParameters { Start = start, Limit = limit }
        );
        
        if (response.Data?.Any() == true)
        {
            allWorkOrders.AddRange(response.Data);
            start += limit;
            
            if (allWorkOrders.Count >= response.TotalCount)
                break;
        }
        else
        {
            break;
        }
    }
    
    return allWorkOrders;
}
```

---

## Configuration Examples

### appsettings.json
```json
{
  "FexaApi": {
    "BaseUrl": "https://aafmapisandbox.fexa.io/",
    "ClientId": "",
    "ClientSecret": "",
    "TokenRefreshBufferSeconds": 300,
    "MaxRetryAttempts": 3
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Fexa.ApiClient": "Debug"
    }
  }
}
```

### User Secrets (Development)
```bash
dotnet user-secrets init
dotnet user-secrets set "FexaApi:ClientId" "your_client_id"
dotnet user-secrets set "FexaApi:ClientSecret" "your_client_secret"
```

### Environment Variables (Production)
```bash
export FexaApi__ClientId="your_client_id"
export FexaApi__ClientSecret="your_client_secret"
export FexaApi__BaseUrl="https://api.fexa.io/"
```

---

## Testing

### Unit Testing Example
```csharp
[Fact]
public async Task GetWorkOrder_ReturnsWorkOrder_WhenIdIsValid()
{
    // Arrange
    var mockApiService = new Mock<IFexaApiService>();
    var expectedWorkOrder = new WorkOrder { Id = 123 };
    
    mockApiService
        .Setup(x => x.GetAsync<WorkOrder>(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedWorkOrder);
    
    var service = new WorkOrderService(
        mockApiService.Object, 
        Mock.Of<ILogger<WorkOrderService>>()
    );
    
    // Act
    var result = await service.GetWorkOrderAsync(123);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(123, result.Id);
}
```

### Integration Testing
```csharp
[Fact]
public async Task FullWorkflow_CreateAndCompleteWorkOrder()
{
    // This would be an integration test against the sandbox API
    var services = new ServiceCollection();
    services.AddFexaApiClient(options =>
    {
        options.BaseUrl = "https://aafmapisandbox.fexa.io/";
        options.ClientId = _configuration["FexaApi:ClientId"];
        options.ClientSecret = _configuration["FexaApi:ClientSecret"];
    });
    
    var provider = services.BuildServiceProvider();
    var workOrderService = provider.GetRequiredService<IWorkOrderService>();
    
    // Test the full workflow
    var workOrders = await workOrderService.GetWorkOrdersAsync(
        new QueryParameters { Limit = 1 }
    );
    
    Assert.NotNull(workOrders);
    Assert.True(workOrders.TotalCount >= 0);
}
```

---

## Troubleshooting

### Common Issues

#### 1. Authentication Failures
- Verify ClientId and ClientSecret are correct
- Check if credentials are for the correct environment (sandbox vs production)
- Ensure secrets are properly loaded in configuration

#### 2. Filter Syntax Errors
- Visit endpoint requires different filter format than other endpoints
- Use "property" instead of "key" for visits
- No prefix needed for visit filter fields

#### 3. Rate Limiting
- Implement exponential backoff
- Cache frequently accessed data
- Use batch operations where available

#### 4. Timeout Issues
- Default timeout is 30 seconds
- Can be configured in HttpClient setup
- Consider implementing Polly policies for resilience

#### 5. Null Reference Exceptions
- Many fields in responses can be null
- Always use null-conditional operators
- Validate responses before processing

---

## Appendix

### A. Status ID Reference

Common Work Order Status IDs:
- 17: Assigned to Vendor
- 11: Accepted
- 206: Response Deadline Missed
- 236: Pending Signoffs / Photos
- 148: Schedule Pending Acceptance

### B. Priority Levels

| ID | Name | Response Time | Arrival Time | Completion Time |
|----|------|--------------|--------------|-----------------|
| 13 | Low - 7 Days | 30 min | 7 days | 14 days |
| 14 | Standard | 30 min | 3 days | 7 days |
| 15 | High - 24 Hours | 30 min | 24 hours | 48 hours |
| 16 | Emergency | 15 min | 4 hours | 8 hours |

### C. Category Examples

| ID | Category | Parent Category |
|----|----------|----------------|
| 130 | Carpentry | Carpentry (323) |
| 285 | Plumbing | Parent Trade General Plumbing |
| 293 | General Maintenance | Maintenance |
| 315 | Grease Trap | Plumbing (285) |

### D. Useful Resources

- [Fexa API Documentation](https://aafmapisandbox.fexa.io/fexa_docs/index.html)
- [OAuth 2.0 Specification](https://oauth.net/2/)
- [JSON API Specification](https://jsonapi.org/)
- [RFC 7231 - HTTP Status Codes](https://tools.ietf.org/html/rfc7231#section-6)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-08-20 | Initial documentation |
| 1.1.0 | 2025-12-15 | Updated for TukaTek organization, filter fixes, enhanced error handling |

---

## Repository Information

- **Repository**: [TukaTek/aafm_fexa_api](https://github.com/TukaTek/aafm_fexa_api)
- **Organization**: [TukaTek](https://github.com/TukaTek)
- **Issues**: [GitHub Issues](https://github.com/TukaTek/aafm_fexa_api/issues)
- **Documentation**: [API Docs](https://aafmapisandbox.fexa.io/fexa_docs/index.html)
- **Latest Updates**: December 2024
- **Maintained by**: TukaTek development team

## Support

For questions and support:
- Create an issue on [GitHub](https://github.com/TukaTek/aafm_fexa_api/issues)
- Contact: [TukaTek](https://github.com/TukaTek)

---

*Generated by analyzing the Fexa API Client implementation for AAFM integration by TukaTek*