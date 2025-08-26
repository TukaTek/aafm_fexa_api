# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
This is a .NET 8.0 API client library and Web API middleware for the Fexa API, providing OAuth 2.0 authenticated access to AAFM (American Association of Fleet Managers) data including users, invoices, visits, work orders, workflow transitions, and reference data.

**Solution Components**:
- Core API Client Library (Fexa.ApiClient)
- ASP.NET Core Web API Middleware (Fexa.ApiClient.WebApi) 
- Interactive Console Application (Fexa.ApiClient.Console)
- Azure Functions (Fexa.ApiClient.Function) - Deprecated due to DI issues

**Azure Deployment**:
- **Resource Group**: aafm_chatcmb
- **App Service**: fexa-api-webapp
- **URL**: https://fexa-api-webapp.azurewebsites.net
- **Swagger**: https://fexa-api-webapp.azurewebsites.net/swagger/v1/swagger.json

## Essential Commands

### Build and Test
```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/Fexa.ApiClient.Tests/

# Run tests with filter
dotnet test --filter "FullyQualifiedName~FilterBuilder"
```

### Development
```bash
# Run the interactive console application
cd src/Fexa.ApiClient.Console
dotnet run

# Download all transitions to JSON
dotnet run download-transitions

# Configure API credentials (required before running)
cd src/Fexa.ApiClient.Console
dotnet user-secrets init
dotnet user-secrets set "FexaApi:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "FexaApi:ClientSecret" "YOUR_CLIENT_SECRET"

# Clean and rebuild
dotnet clean
dotnet build --no-incremental
```

## Architecture Overview

### Solution Structure
- **src/Fexa.ApiClient/**: Core library implementing the API client
- **src/Fexa.ApiClient.WebApi/**: ASP.NET Core Web API middleware (primary deployment)
- **src/Fexa.ApiClient.Console/**: Interactive console application for testing/demonstration
- **src/Fexa.ApiClient.Function/**: Azure Functions (deprecated - replaced by WebApi)
- **tests/Fexa.ApiClient.Tests/**: Unit tests using xUnit, FluentAssertions, and Moq

### Key Architectural Components

#### Service Layer Architecture
The client uses dependency injection with service interfaces:
- `IFexaApiService`: Core HTTP operations with retry/circuit breaker via Polly
- `ITokenService`: OAuth 2.0 token management with automatic refresh
- `IUserService`, `IClientInvoiceService`, `IVisitService`: Domain-specific API endpoints
- `IWorkOrderService`: Work order operations including status updates
- `ITransitionService`: Workflow transition management with caching
- `IRegionService`: Region data operations
- `ISeverityService`: Severity level operations
- `INoteService`: Note management operations (updated for work order notes)
- `IDocumentTypeService`: Document type operations with caching
- `IPriorityService`: Priority management with caching
- `IWorkOrderCategoryService`: Work order category operations with hierarchy support
- `IClientService`: Client information management
- `IVendorService`: Vendor operations
- `ILocationService`: Location/store operations with caching

#### Authentication Flow
- OAuth 2.0 Client Credentials flow implemented in `FexaAuthenticationHandler`
- Automatic token refresh on 401 responses
- Token caching to minimize authentication requests
- Thread-safe token acquisition

#### Configuration System
- Strongly-typed options via `FexaApiOptions` with validation
- Supports multiple configuration sources (appsettings.json, user secrets, environment variables)
- Configuration validation on startup

#### Filter System
The `FilterBuilder` class provides a fluent API for building complex Fexa API filters:
- Supports all Fexa filter operators (equals, in, not in, between, date ranges)
- Type-safe filter construction
- Automatic JSON serialization to Fexa's filter format
- `AddFilters` method for bulk filter addition
- Convenience methods for common filters (WhereWorkOrderId, WhereVendorId, etc.)

#### Exception Hierarchy
- `FexaApiException`: Base exception with request context
- `FexaAuthenticationException`: Auth failures
- `FexaRateLimitException`: Rate limit handling with retry-after
- `FexaValidationException`: Validation errors

### API Integration Points
- **Fexa Base URL**: https://aafmapisandbox.fexa.io/ (sandbox)
- **Fexa Documentation**: https://aafmapisandbox.fexa.io/fexa_docs/index.html
- **Authentication Endpoint**: /oauth/token
- **Fexa Endpoints**: 
  - /api/v2/{endpoint} (users, clients, invoices)
  - /api/ev1/{endpoint} (visits, work orders, transitions, notes, categories, priorities, document types)
- **Our Web API**: https://fexa-api-webapp.azurewebsites.net
- **Swagger Documentation**: https://fexa-api-webapp.azurewebsites.net/swagger/v1/swagger.json

### Important Implementation Details

1. **HTTP Client Configuration**: Uses typed HttpClient with Polly policies for resilience (exponential backoff retry, circuit breaker)

2. **Response Handling**: All API responses wrapped in `BaseResponse<T>` or `PagedResponse<T>` with standardized error handling

3. **Service Registration**: Extension method `AddFexaApiClient()` registers all services with proper lifetimes:
   - Scoped: Core API services (IFexaApiService, ITokenService, domain services)
   - Singleton: Reference data services with caching (DocumentType, Priority, WorkOrderCategory)
   - Memory cache configured with 1-hour expiration for reference data

4. **Testing Approach**: Unit tests focus on FilterBuilder logic and service behavior with mocked dependencies

5. **Logging**: Integrated with Microsoft.Extensions.Logging for comprehensive debugging

6. **Visits Endpoint Specifics**:
   - Uses `/api/ev1/visits` endpoint (not v2)
   - Requires filters as URL-encoded JSON array in query parameters
   - Filter fields use no prefix (e.g., `"start_date"` not `"visits.start_date"`)
   - Uses `"property"` field name in filters (not `"key"`)
   - Date filtering uses datetime ranges for proper inclusion (00:00:00 to 23:59:59)
   - Direct query parameters (workorder_id=X) work for some filters
   - Accept: application/json header is required for all API calls

7. **Work Orders Endpoint Specifics**:
   - Uses `/api/ev1/workorders` endpoint
   - Filters must be URL-encoded JSON arrays with `property`, `operator`, and `value` fields
   - Vendor filtering uses `"vendors.id"` property
   - Client filtering uses `"clients.id"` property
   - Status filtering uses `"object_state.status.name"` property
   - Technician filtering uses `"lead_technician_role_id"` property
   - Date range filtering uses `"created_at"` with between operator

8. **Work Order Status Updates**:
   - Uses `/api/ev1/workorders/{id}/update_status/{statusId}` endpoint
   - **NO request body is sent** - status ID is in URL path only
   - Returns `{"success":true}` on success
   - May return 400 Bad Request if transition requirements not met
   - Error: "Either the workflow transition specified does not exist, or you do not meet the requirements to utilize it"

9. **Workflow Transitions**:
   - **IMPORTANT**: Use "Work Order" as workflow_object_type, NOT "Assignment"
   - Transitions define valid status changes but business rules may prevent them
   - Cache transitions for 30 minutes to improve performance
   - Different entities have different workflow types (Work Order, Assignment, Visit, etc.)

10. **Error Detection Fix**:
   - Check for `"success":true` or `"success":false` in responses
   - Do NOT check for presence of "error" string (false positives from "errors" array)
   - Routing errors contain "routing_error" string

11. **Note Service Updates**:
   - For work order notes, use `type: "Notes::WorkorderNote"` instead of `note_type_id`
   - CreateNoteForWorkOrderAsync method specifically handles work order note creation
   - General notes still use `note_type_id` parameter

12. **Reference Data Services** (with 1-hour memory caching):
   - DocumentTypeService: Manages document types from `/api/ev1/document_types`
   - PriorityService: Handles priorities from `/api/ev1/priorities`
   - WorkOrderCategoryService: Categories with hierarchy from `/api/ev1/workorder_categories`
   - All registered as Singleton services for efficient memory usage

13. **Work Order Client PO Search**:
   - Single endpoint: `/api/WorkOrder/clientpo/{poNumber}` 
   - Automatically fetches ALL matching work orders (handles pagination internally)
   - Uses `purchase_order_number` filter with "in" operator
   - Note: `workorder_number` field may be null in API responses

14. **Location Service** (Stores):
   - Uses `/api/ev1/stores` endpoint
   - Filter by client using `occupied_by` field
   - Single location returns `{"stores": {...}}` not `{"store": {...}}`
   - Nullable fields: `flag`, `default_address`, `import_id` (can be int or null)
   - Includes full address, geographic coordinates, and timezone information
   - 1-hour memory caching for performance

### Development Guidelines

When adding new API endpoints:
1. Create interface in `/Services/Interfaces/`
2. Implement in `/Services/`
3. Add models in `/Models/`
4. Register in `ServiceCollectionExtensions`
5. Add unit tests in corresponding test project
6. Test with console application menu system

When modifying filter capabilities:
1. Update `FilterBuilder` class
2. Add corresponding unit tests
3. Ensure backward compatibility

### Model Classes

**Core Models**:
- `User` (in ExampleModels.cs) - Basic user information
- `NoteUser` (in Note.cs) - User information specific to notes
- `Note` - Note/comment data with user association
- `WorkOrder` - Work order data with status and assignments
- `Visit` - Service visit information
- `Transition` - Workflow state transitions
- `Region`, `Severity` - Reference data models
- `DocumentType` - Document type definitions
- `Priority` - Priority levels with severity information
- `WorkOrderCategory` - Hierarchical category structure
- `Client` - Client organization information
- `Location` - Store/location information with addresses and geographic data

**Simplified DTOs** (for Web API responses):
- `DocumentTypeDto` - id, name, description
- `PriorityDto` - id, name, description
- `WorkOrderCategoryDto` - id, category, description, parent_category, full_path
- `ClientDto` - id, company, dba, active, ivr_id, address info, cmms_prog, invoicing_method, taxable
- `LocationDto` - id, name, identifier, facility_code, address, geographic coordinates, timezone

### Common Tasks

**Adding a new API endpoint:**
```csharp
// 1. Define in interface
public interface INewService
{
    Task<BaseResponse<NewModel>> GetAsync(int id);
}

// 2. Implement service
public class NewService : INewService
{
    // Use IFexaApiService for HTTP operations
}

// 3. Register in ServiceCollectionExtensions
services.AddScoped<INewService, NewService>();
// For reference data services with caching:
services.AddSingleton<INewService, NewService>();
```

**Using FilterBuilder:**
```csharp
// Basic filter building
var filter = new FilterBuilder()
    .Where("status", "active")
    .WhereIn("type", "A", "B")
    .WhereDateBetween("created", startDate, endDate)
    .Build();

// Using convenience methods
var filter = new FilterBuilder()
    .WhereVendorId(443210)
    .WhereWorkOrderStatus("New")
    .Build();

// Adding multiple filters at once
var existingFilters = new List<FexaFilter>();
var filter = new FilterBuilder()
    .AddFilters(existingFilters)
    .Where("additional_field", "value")
    .Build();
```

**Work Orders Endpoint Filter Format:**
```csharp
// Work orders use URL-encoded JSON array filters
var filters = new List<FexaFilter>
{
    new FexaFilter("vendors.id", 443210),
    new FexaFilter("object_state.status.name", "New")
};

// Filters are automatically formatted by WorkOrderService
var response = await workOrderService.GetWorkOrdersByVendorAsync(443210);
```

**Visits Endpoint Filter Format:**
```csharp
// Visits endpoint requires different filter format
var filter = new[]
{
    new Dictionary<string, object>
    {
        ["property"] = "start_date",  // No prefix needed
        ["operator"] = "between",
        ["value"] = new[] { "2025-08-14 00:00:00", "2025-08-15 23:59:59" }
    }
};
var filterJson = JsonSerializer.Serialize(filter);
var encodedFilter = HttpUtility.UrlEncode(filterJson);
var endpoint = $"/api/ev1/visits?filters={encodedFilter}";
```

**Work Order Status Update:**
```csharp
// Status ID goes in URL, no body
var endpoint = $"/api/ev1/workorders/{workOrderId}/update_status/{newStatusId}";
var response = await _apiService.PutAsync<dynamic>(endpoint, null); // null body!
```

**Note Creation for Work Orders:**
```csharp
// Use CreateNoteForWorkOrderAsync for work order notes
var note = await noteService.CreateNoteForWorkOrderAsync(
    workOrderId: 12345,
    content: "Work completed successfully",
    visibility: "all",
    actionRequired: false
);
// This uses type: "Notes::WorkorderNote" internally
```

### Console Application Menu System

The console app provides comprehensive testing capabilities:

1. **Main Menu** - Entry point with all service options
2. **Work Order Menu** - Test all work order operations
3. **Transition Testing** - Verify workflow transitions
4. **Status Update** - Interactive status changes with validation
5. **Debug Tools** - Advanced API debugging features

Key menu classes:
- `MenuSystem.cs` - Main menu orchestration
- `TestWorkOrderDebug.cs` - Work order API debugging
- `TestTransitionFix.cs` - Transition verification
- `TestStatusUpdateFix.cs` - Direct status update testing
- `DownloadTransitions.cs` - Export transitions to JSON

### Migration History

**December 2024**: 
- Migrated from Azure Functions to ASP.NET Core Web API due to DI issues
- Added reference data services with memory caching
- Simplified DTOs for cleaner API responses
- Fixed work order note creation to use correct type field
- Deployed to Azure App Service (fexa-api-webapp)

**August 2025**:
- Added Location service for store/location management
- Implemented location search by client ID
- Added geographic and timezone support for locations

### Deployment & Publishing

**Azure Web App Deployment**:
```bash
# Build and publish locally
cd /Users/sanjay/Source/repos/aafm_fexa_api/FexaApiClient
rm -rf ./webapi-publish
dotnet publish src/Fexa.ApiClient.WebApi -c Release -o ./webapi-publish

# Create deployment package
cd webapi-publish
zip -r ../webapi-deploy.zip .

# Deploy to Azure (only when explicitly requested)
az webapp deploy --resource-group aafm_chatcmb --name fexa-api-webapp \
  --src-path ../webapi-deploy.zip --type zip --restart true
```

**Configuration in Azure**:
- App Settings use double underscore format: `FexaApi__PropertyName`
- Startup command: `dotnet Fexa.ApiClient.WebApi.dll`
- Runtime: DOTNETCORE|8.0

**Important**: Do NOT auto-deploy. Only publish when explicitly requested by the user.

### Web API Endpoints (Deployed to Azure)

The Web API provides simplified endpoints with DTOs at https://fexa-api-webapp.azurewebsites.net

**Document Types** (`/api/DocumentType`):
- GET `/` - Get all document types (cached 1 hour)
- GET `/active` - Get only active document types
- Returns: DocumentTypeDto (id, name, description)

**Priorities** (`/api/Priority`):
- GET `/` - Get all priorities (cached 1 hour)
- GET `/active` - Get only active priorities
- Returns: PriorityDto (id, name, description)

**Work Order Categories** (`/api/WorkOrderCategory`):
- GET `/` - Get all categories (cached 1 hour)
- GET `/hierarchy` - Get categories with parent-child relationships
- Returns: WorkOrderCategoryDto (id, category, description, parent_category, full_path)

**Clients** (`/api/Client`):
- GET `/` - Get paginated clients
- GET `/{id}` - Get specific client
- GET `/active` - Get only active clients
- GET `/all` - Get all clients (handles pagination internally)
- Returns: ClientDto (simplified client information)

**Work Orders** (`/api/WorkOrder`):
- GET `/{id}` - Get specific work order
- GET `/vendor/{vendorId}` - Get work orders by vendor
- GET `/client/{clientId}` - Get work orders by client
- GET `/clientpo/{poNumber}` - Get ALL work orders by Client PO (auto-pagination)
- Returns: WorkOrder model

**Notes** (`/api/Note`):
- GET `/` - Get paginated notes
- GET `/{noteId}` - Get specific note
- GET `/object/{objectType}/{objectId}` - Get notes for an object
- POST `/` - Create a note
- POST `/workorder/{workOrderId}` - Create work order note (uses type: "Notes::WorkorderNote")
- PUT `/{noteId}` - Update a note
- DELETE `/{noteId}` - Delete a note
- Returns: Note model

**Locations** (`/api/Location`):
- GET `/{id}` - Get specific location (cached 1 hour)
- GET `/client/{clientId}` - Get all locations for a client (cached 1 hour)
- GET `/` - Get all locations
- GET `/active` - Get only active locations
- Returns: LocationDto (simplified location information with address and coordinates)
- GET `/{id}` - Get document type by ID
- GET `/byname/{name}` - Get document type by name

**Priorities** (`/api/Priority`):
- GET `/` - Get all priorities
- GET `/active` - Get only active priorities
- GET `/{id}` - Get priority by ID
- GET `/byname/{name}` - Get priority by name

**Work Order Categories** (`/api/WorkOrderCategory`):
- GET `/` - Get all categories
- GET `/active` - Get only active categories
- GET `/leaf` - Get only leaf categories (no children)
- GET `/parent` - Get only parent categories (top-level)
- GET `/{id}` - Get category by ID
- GET `/byname/{name}` - Get category by name

**Clients** (`/api/Client`):
- GET `/` - Get paged list of clients
- GET `/{id}` - Get client by ID
- GET `/all` - Get all clients (with max pages limit)

**Other Endpoints**:
- Vendors, Work Orders, Visits, Transitions, Notes, Users - all available with various operations

### Recent Fixes and Improvements

1. **Work Order Filter Fix (December 2024)**:
   - Fixed vendor filtering to use `"vendors.id"` instead of `"assigned_to"`
   - Fixed client filtering to use `"clients.id"` instead of `"placed_for"`
   - Corrected filter format to use URL-encoded JSON arrays
   - Filters now properly use `property`, `operator`, and `value` structure

2. **Model Namespace Fix (December 2024)**:
   - Renamed `User` class in Note.cs to `NoteUser` to avoid conflicts
   - Added `AddFilters` method to FilterBuilder for bulk filter operations

3. **Work Order Status Update Fix**:
   - Changed from body-based to URL-based status ID
   - Fixed endpoint format to `/update_status/{statusId}`
   - Removed request body completely

4. **Workflow Type Correction**:
   - Fixed filtering to use "Work Order" not "Assignment"
   - Corrected transition type detection
   - Updated all services to use correct workflow_object_type

5. **Notes Endpoint Specifics**:
   - Uses `/api/ev1/notes` endpoint
   - Work order notes require `type: "Notes::WorkorderNote"` in payload
   - Do NOT use `note_type_id` for work order notes
   - Other note types still use `note_type_id` field
   - Filters use `notable_id` for object ID filtering

6. **Error Detection Improvement**:
   - Fixed false positives from "errors" array
   - Now checks for explicit "success":true/false
   - Better handling of routing errors

7. **Performance Optimizations**:
   - Added 30-minute caching for transitions
   - Bulk operations for fetching all pages
   - Efficient service scoping

8. **Console App Enhancements**:
   - Added warning about transition requirements
   - Improved error messages
   - Added transition download capability
   - Interactive debugging tools

9. **Work Order Note Creation Fix (December 2024)**:
   - Changed payload for work order notes to use `type: "Notes::WorkorderNote"`
   - Removed `note_type_id` field for work order notes
   - Maintained backward compatibility for other note types

10. **Web API Implementation (December 2024)**:
   - Created ASP.NET Core Web API to replace Azure Functions
   - Added simplified DTOs for all major entities
   - Implemented caching for reference data (document types, priorities, categories)
   - Deployed to Azure App Service at fexa-api-webapp

### Testing Strategy

1. **Unit Tests**: Focus on core logic (filters, builders)
2. **Integration Tests**: Use console app for API testing
3. **Manual Testing**: Interactive menu for complex scenarios
4. **Debug Tools**: Built-in debugging options in console app

### Known Limitations

1. **Status Transitions**: Some transitions require specific conditions (completed fields, permissions) that aren't visible in the transition list
2. **Work Order Status**: May be computed from assignment statuses, not directly updatable in all cases
3. **API Documentation**: Some endpoints aren't documented or have incorrect documentation
4. **Filter Formats**: Different endpoints use slightly different filter formats

### Troubleshooting Guide

**Problem**: Status update fails with "workflow transition does not exist"
**Solution**: The transition exists but has unmet requirements. Check work order state, assignments, and required fields.

**Problem**: Getting wrong workflow transitions
**Solution**: Ensure filtering by "Work Order" not "Assignment" workflow_object_type.

**Problem**: API returns success but status didn't change
**Solution**: Check response for actual success:true flag, not just 200 status code.

**Problem**: Filters not working
**Solution**: Verify filter format matches endpoint requirements (visits use different format than other endpoints).

**Problem**: Work order vendor filter returns wrong results
**Solution**: Ensure using `"vendors.id"` property (not `"assigned_to"`) and filters are URL-encoded JSON arrays.

**Problem**: Duplicate class definition errors
**Solution**: Check for naming conflicts - Note.cs uses `NoteUser` to avoid conflict with `User` in ExampleModels.cs.

### Best Practices

1. Always handle transition failures gracefully
2. Cache transitions to reduce API calls
3. Use the console app for testing before production
4. Check for success:true in responses, not just HTTP status
5. Be aware that visible transitions may have hidden requirements
6. Use proper service scoping (CreateScope for scoped services)
7. URL-encode filter JSON when passing as query parameters
8. Include time components in date filters (00:00:00 to 23:59:59)

## Repository Information

- **Repository**: [TukaTek/aafm_fexa_api](https://github.com/TukaTek/aafm_fexa_api)
- **Organization**: [TukaTek](https://github.com/TukaTek)
- **Issues**: [GitHub Issues](https://github.com/TukaTek/aafm_fexa_api/issues)
- **Latest Updates**: December 2024
- **Maintained by**: TukaTek development team