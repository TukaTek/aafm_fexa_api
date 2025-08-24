# Fexa API Client

A scalable, reusable, and secure .NET client library for accessing the Fexa API (AAFM - American Association of Fleet Managers).

**API Documentation**: https://aafmapisandbox.fexa.io/fexa_docs/index.html

## Features

- **Scalable Architecture**: Service-oriented design with dependency injection
- **Resilient HTTP Client**: Built-in retry policies and circuit breaker using Polly
- **Secure**: OAuth 2.0 client credentials flow with automatic token management
- **Extensible**: Easy to add new API endpoints and services
- **Comprehensive Error Handling**: Custom exceptions for different error scenarios
- **Logging**: Integrated logging support using Microsoft.Extensions.Logging
- **Configuration**: Flexible configuration using IOptions pattern
- **Multiple Services**: Support for Users, Invoices, Visits, Work Orders, Transitions, Regions, Severities, and Notes
- **Advanced Filtering**: Powerful filter builder supporting multiple operators and date ranges with bulk filter operations
- **Workflow Management**: Complete support for status transitions and workflow states
- **Performance Optimized**: Caching for frequently accessed data like transitions

## Solution Structure

```
FexaApiClient/
├── src/
│   ├── Fexa.ApiClient/              # Core library
│   │   ├── Configuration/           # API settings and options
│   │   ├── Services/               # Service implementations
│   │   ├── Models/                 # DTOs and request/response models
│   │   ├── Exceptions/             # Custom exceptions
│   │   ├── Http/                   # HTTP handlers and client setup
│   │   └── Extensions/             # DI and service extensions
│   └── Fexa.ApiClient.Console/      # Interactive test console application
└── tests/
    └── Fexa.ApiClient.Tests/        # Unit tests
```

## Quick Start

1. Clone the repository
2. Navigate to the console application directory:
   ```bash
   cd src/Fexa.ApiClient.Console
   ```
3. Set your credentials using user secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "FexaApi:ClientId" "YOUR_ACTUAL_CLIENT_ID"
   dotnet user-secrets set "FexaApi:ClientSecret" "YOUR_ACTUAL_CLIENT_SECRET"
   ```
4. Run the interactive console application:
   ```bash
   dotnet run
   ```

## Console Application Features

The included console application provides an interactive menu system for testing all API features:

### Main Menu Options
1. **Test Fexa API Connection** - Verify authentication and connectivity
2. **Test Visit Service** - Query and filter visits
3. **Test Work Order Service** - Comprehensive work order operations
4. **Debug Work Order API** - Advanced debugging tools for work order filters
5. **Get Workflow Statuses & Transitions** - Explore workflow states and transitions
6. **Test Transition Service** - Performance testing with caching
7. **Debug Work Order Statuses** - Detailed status debugging
8. **Update Work Order Status** - Interactive status updates with transition validation
9. **Download All Transitions** - Export all transitions to JSON
10. **Test Action Required Transitions** - Verify specific transition paths
11. **Test Direct Status Update** - Debug status update API calls

### Command Line Options
```bash
# Download all transitions to JSON file
dotnet run download-transitions
```

## Getting Started

### Installation

1. Add the Fexa.ApiClient project reference to your application:
```xml
<ProjectReference Include="path/to/Fexa.ApiClient.csproj" />
```

2. Configure services in your application:
```csharp
services.AddFexaApiClient(configuration);
```

### Configuration

Add the following to your `appsettings.json`:

```json
{
  "FexaApi": {
    "BaseUrl": "https://aafmapisandbox.fexa.io/",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE",
    "TokenEndpoint": "/oauth/token",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "EnableLogging": true,
    "TokenRefreshBufferSeconds": 300
  }
}
```

### Using User Secrets (Recommended for Development)

To securely store your credentials during development:

```bash
cd src/Fexa.ApiClient.Console
dotnet user-secrets init
dotnet user-secrets set "FexaApi:ClientId" "YOUR_ACTUAL_CLIENT_ID"
dotnet user-secrets set "FexaApi:ClientSecret" "YOUR_ACTUAL_CLIENT_SECRET"
```

## Available Services

### Work Order Service
- Query work orders with pagination and filtering
- Get individual work orders by ID
- Filter by status, vendor (using `vendors.id`), client (`clients.id`), technician, and date ranges
- Update work order status (with transition validation)
- Support for bulk operations across multiple pages
- Proper URL-encoded JSON array filter format for all queries

### Transition Service
- Get all workflow transitions with caching
- Query transitions by status (from/to)
- Get work order-specific statuses
- Validate status transitions before updates
- Performance optimized with in-memory caching

### Visit Service
- Query visits with advanced filtering
- Filter by work order, technician, client, location, and status
- Comprehensive date filtering:
  - Specific date (scheduled or actual)
  - Date ranges
  - Before/after date queries
- Support for complex multi-criteria queries

### User Service
- Create, read, update, and delete users
- Query users with pagination and filtering
- Full CRUD operations support

### Client Invoice Service  
- Query client invoices
- Filter by work order, vendor, and date ranges
- Pagination support for large result sets

### Region Service
- Query regions with pagination and filtering
- Support for bulk filter operations
- Full filtering capabilities with FilterBuilder integration

### Severity Service
- Query severity levels with pagination and filtering
- Support for bulk filter operations
- Full filtering capabilities with FilterBuilder integration

### Note Service
- Query notes with pagination and filtering
- Support for bulk filter operations
- Full filtering capabilities with FilterBuilder integration
- User association with NoteUser model

## Usage Examples

### Work Order Service
```csharp
public class WorkOrderManager
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ITransitionService _transitionService;
    
    public WorkOrderManager(IWorkOrderService workOrderService, ITransitionService transitionService)
    {
        _workOrderService = workOrderService;
        _transitionService = transitionService;
    }
    
    // Get work orders by status
    public async Task<List<WorkOrder>> GetWorkOrdersByStatusAsync(string status)
    {
        return await _workOrderService.GetAllWorkOrdersByStatusAsync(status);
    }
    
    // Update work order status with validation
    public async Task<WorkOrder> UpdateStatusAsync(int workOrderId, int newStatusId)
    {
        // Note: The API validates transitions and may reject invalid status changes
        // even if the transition exists in the system (business rules apply)
        return await _workOrderService.UpdateStatusAsync(workOrderId, newStatusId);
    }
    
    // Get valid transitions for a work order
    public async Task<List<WorkflowTransition>> GetValidTransitionsAsync(int currentStatusId)
    {
        var allTransitions = await _transitionService.GetAllTransitionsAsync();
        return allTransitions
            .Where(t => t.WorkflowObjectType == "Work Order" && t.FromStatusId == currentStatusId)
            .ToList();
    }
}
```

### Transition Service
```csharp
public class TransitionManager
{
    private readonly ITransitionService _transitionService;
    
    public TransitionManager(ITransitionService transitionService)
    {
        _transitionService = transitionService;
    }
    
    // Get all work order statuses
    public async Task<List<WorkflowStatus>> GetWorkOrderStatusesAsync()
    {
        return await _transitionService.GetWorkOrderStatusesAsync();
    }
    
    // Check if a transition is valid
    public async Task<bool> IsTransitionValidAsync(int fromStatusId, int toStatusId)
    {
        var transitions = await _transitionService.GetTransitionsFromStatusAsync(fromStatusId);
        return transitions.Any(t => 
            t.WorkflowObjectType == "Work Order" && 
            t.ToStatusId == toStatusId);
    }
}
```

### Visit Service
```csharp
public class VisitManager
{
    private readonly IVisitService _visitService;
    
    public VisitManager(IVisitService visitService)
    {
        _visitService = visitService;
    }
    
    // Get visits for today
    public async Task<PagedResponse<Visit>> GetTodaysVisitsAsync()
    {
        return await _visitService.GetVisitsByScheduledDateAsync(DateTime.Today);
    }
    
    // Get visits for a specific technician
    public async Task<PagedResponse<Visit>> GetTechnicianVisitsAsync(int technicianId)
    {
        return await _visitService.GetVisitsByTechnicianAsync(technicianId);
    }
    
    // Complex filtering example
    public async Task<PagedResponse<Visit>> GetFilteredVisitsAsync()
    {
        var parameters = QueryParameters.Create()
            .WithFilters(filters => filters
                .WhereScheduledAfter(DateTime.Today)
                .WhereVisitStatus("scheduled")
                .WhereTechnicianId(42));
                
        return await _visitService.GetVisitsAsync(parameters);
    }
}
```

## Workflow Management

### Understanding Work Order Statuses

The Fexa API uses a complex workflow system with transitions between statuses. Key concepts:

1. **Workflow Object Types**: Different entities have different workflows
   - Work Order
   - Assignment
   - Visit
   - Client Invoice
   - Vendor Invoice
   - etc.

2. **Status Transitions**: Not all status changes are allowed
   - Transitions define valid from → to status paths
   - Business rules may prevent transitions even if they exist
   - Some transitions require specific conditions (completed fields, permissions, etc.)

3. **Status Update Behavior**:
   - Work order status updates use: `PUT /api/ev1/workorders/{id}/update_status/{statusId}`
   - No request body is sent
   - The API validates the transition and may reject it with a 400 error
   - Error message: "Either the workflow transition specified does not exist, or you do not meet the requirements to utilize it"

### Example: Working with Transitions
```csharp
// Get all transitions for work orders
var allTransitions = await transitionService.GetAllTransitionsAsync();
var workOrderTransitions = allTransitions
    .Where(t => t.WorkflowObjectType == "Work Order")
    .ToList();

// Find valid next statuses from current status
var currentStatusId = 87; // "Action Required"
var validNextStatuses = workOrderTransitions
    .Where(t => t.FromStatusId == currentStatusId)
    .Select(t => new { t.ToStatusId, t.ToStatus.Name })
    .Distinct()
    .ToList();

// Note: Even if a transition exists, the API may reject it based on business rules
// Always handle potential failures when updating status
try
{
    var updated = await workOrderService.UpdateStatusAsync(workOrderId, newStatusId);
    Console.WriteLine($"Status updated to: {updated.ObjectState?.Status?.Name}");
}
catch (Exception ex)
{
    Console.WriteLine($"Status update failed: {ex.Message}");
    // The work order may have requirements that prevent this transition
}
```

## API Endpoints

### Key Endpoints Used

- **Authentication**: `POST /oauth/token`
- **Work Orders**: 
  - List: `GET /api/ev1/workorders` (with URL-encoded JSON filters)
  - Get: `GET /api/ev1/workorders/{id}`
  - Update Status: `PUT /api/ev1/workorders/{id}/update_status/{statusId}` (no request body)
- **Visits**: `GET /api/ev1/visits` (different filter format than work orders)
- **Transitions**: `GET /api/ev1/users/list_transitions`
- **Users**: `GET/POST/PUT/DELETE /api/ev1/users`
- **Invoices**: `GET /api/v2/invoices`
- **Regions**: `GET /api/v2/regions`
- **Severities**: `GET /api/v2/severities`
- **Notes**: `GET /api/v2/notes`

### Filter Format

The Fexa API uses a specific filter format that varies by endpoint:

#### Work Orders and Most Endpoints
```json
[
  {
    "property": "vendors.id",
    "operator": "equals",
    "value": 443210
  },
  {
    "property": "created_at",
    "operator": "between",
    "value": ["2025-08-01 00:00:00", "2025-08-31 23:59:59"]
  }
]
```

#### Visits Endpoint (Different Format)
```json
[
  {
    "property": "start_date",
    "operator": "between",
    "value": ["2025-08-14 00:00:00", "2025-08-15 23:59:59"]
  }
]
```

Filters are URL-encoded and passed as query parameters. Note that:
- Work orders use prefixed properties (e.g., `vendors.id`, `clients.id`)
- Visits use unprefixed properties (e.g., `start_date` not `visits.start_date`)
- The FilterBuilder class handles this automatically

## Error Handling

The client provides specific exception types for different scenarios:

- `FexaApiException`: General API errors
- `FexaAuthenticationException`: Authentication/authorization failures
- `FexaRateLimitException`: Rate limit exceeded
- `FexaValidationException`: Request validation errors

Example error handling:
```csharp
try
{
    var workOrder = await workOrderService.UpdateStatusAsync(workOrderId, newStatusId);
}
catch (FexaValidationException ex)
{
    // Handle validation error (e.g., invalid transition)
    Console.WriteLine($"Validation failed: {ex.Message}");
}
catch (FexaAuthenticationException ex)
{
    // Handle authentication error
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (FexaApiException ex)
{
    // Handle other API errors
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Authentication

The client uses OAuth 2.0 client credentials flow for authentication:

1. **Automatic Token Management**: The client automatically acquires and refreshes access tokens
2. **Token Caching**: Tokens are cached and reused until they expire
3. **Proactive Refresh**: Tokens are refreshed 5 minutes before expiration (configurable)
4. **Thread-Safe**: Token acquisition is thread-safe for concurrent requests
5. **Retry on 401**: Automatically refreshes token and retries on authentication failures

## Performance Optimization

### Caching
- Transitions are cached in memory for 30 minutes (configurable)
- Token caching reduces authentication requests
- Service instances are scoped appropriately for optimal performance

### Resilience
- Exponential backoff retry policy for transient failures
- Circuit breaker pattern to prevent cascading failures
- Configurable timeout settings

### Bulk Operations
- Support for fetching all pages of paginated data
- Efficient batch processing for large datasets

## Troubleshooting

### Common Issues

1. **Status Update Fails with "workflow transition does not exist"**
   - The transition may require specific conditions (completed fields, permissions)
   - Check work order state and assignments
   - Some statuses like "OPS Completed" may require work to be actually completed

2. **Authentication Failures**
   - Verify ClientId and ClientSecret are correct
   - Check token endpoint configuration
   - Ensure credentials are properly stored in user secrets

3. **Filter Not Working**
   - Verify filter format matches endpoint requirements
   - Work orders: Use prefixed properties (`vendors.id`, `clients.id`, `object_state.status.name`)
   - Visits: Use unprefixed properties (`start_date`, `technician_id`)
   - Date filters must include time components (00:00:00 to 23:59:59)
   - Ensure filters are URL-encoded JSON arrays

4. **Vendor Filter Returns Wrong Results**
   - Use `vendors.id` property (not `assigned_to`)
   - Ensure filters are properly formatted as URL-encoded JSON arrays

5. **Build Errors - Duplicate Class Definitions**
   - Note.cs uses `NoteUser` class to avoid conflict with `User` in ExampleModels.cs

## Security Considerations

1. **Never commit credentials** to source control
2. Use user secrets or environment variables for development
3. Use secure key management services (Azure Key Vault, AWS Secrets Manager) in production
4. The client automatically handles OAuth 2.0 authentication
5. Access tokens are automatically added to all API requests
6. SSL/TLS is enforced for all API communications

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Update documentation as needed
4. Ensure all tests pass before submitting PR
5. Test with the interactive console application

## Recent Updates (December 2024)

- **Filter System Fixes**:
  - Fixed vendor filtering to use `vendors.id` instead of `assigned_to`
  - Fixed client filtering to use `clients.id` instead of `placed_for`
  - Corrected work order filter format to use URL-encoded JSON arrays
  - Added `AddFilters` method to FilterBuilder for bulk filter operations

- **Model Updates**:
  - Renamed `User` class in Note.cs to `NoteUser` to avoid namespace conflicts
  - Added Region, Severity, and Note service implementations

- **Previous Updates**:
  - Fixed work order status update to use correct API endpoint format
  - Corrected workflow_object_type filtering ("Work Order" not "Assignment")
  - Improved error detection for API responses
  - Added comprehensive transition management
  - Enhanced console application with debugging tools
  - Optimized performance with caching for transitions
  - Added support for downloading all transitions to JSON

## License

[Your License Here]