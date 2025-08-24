# Visit and Work Order Service Troubleshooting Guide

## Current Status (December 2024)
- Visit Service is working correctly with the `/api/ev1/visits` endpoint
- Work Order Service is working correctly with the `/api/ev1/workorders` endpoint

## Known Issues and Solutions

### Issue 1: Visit Filters Require Special Format

#### Problem
The visits endpoint uses a different filter format than other endpoints.

#### Solution
Visit filters must use unprefixed property names and specific format:
- Use `"start_date"` not `"visits.start_date"`
- Use `"property"` field name in filters (not `"key"`)
- Date filtering must include time components (00:00:00 to 23:59:59)

Example:
```json
[
  {
    "property": "start_date",
    "operator": "between",
    "value": ["2025-08-14 00:00:00", "2025-08-15 23:59:59"]
  }
]
```

### Issue 2: Work Order Vendor Filter

#### Problem
Work order vendor filtering was using incorrect field name.

#### Solution (Fixed December 2024)
- Use `"vendors.id"` property (not `"assigned_to"`)
- Use `"clients.id"` property (not `"placed_for"`)
- Filters must be URL-encoded JSON arrays

Example:
```json
[
  {
    "property": "vendors.id",
    "operator": "equals",
    "value": 443210
  }
]
```

### Issue 3: Duplicate Class Definitions

#### Problem
Build error CS0101: The namespace already contains a definition for 'User'

#### Solution (Fixed December 2024)
- Renamed `User` class in Note.cs to `NoteUser`
- Avoids conflict with `User` class in ExampleModels.cs

### How to Use ConfigurableVisitService

If you need to change the visits endpoint, you can use the ConfigurableVisitService:

1. Add the endpoint to your `appsettings.json`:
```json
{
  "FexaApi": {
    "VisitsEndpoint": "/api/ev1/visits"
  }
}
```

2. Replace the service registration in `ServiceCollectionExtensions.cs`:
```csharp
// Replace this:
services.AddScoped<IVisitService, VisitService>();

// With this:
services.AddScoped<IVisitService, ConfigurableVisitService>();
```

### Debug Tools Available

The console application includes several debug tools:
- `TestVisitDebug.cs` - Tests different possible endpoints
- `DirectApiTest.cs` - Makes raw HTTP calls to verify endpoints
- `TestWorkOrderDebug.cs` - Debug work order filters and queries
- `TestFilterDebug.cs` - Test filter building and formatting

### Common Filter Properties

#### Work Orders
- `vendors.id` - Filter by vendor ID
- `clients.id` - Filter by client ID
- `object_state.status.name` - Filter by status name
- `lead_technician_role_id` - Filter by technician
- `created_at` - Filter by creation date

#### Visits
- `start_date` - Scheduled start date (no prefix)
- `technician_id` - Assigned technician (no prefix)
- `status` - Visit status (no prefix)
- `workorder_id` - Related work order (no prefix)

### Testing Filters

Run the console app to test filters interactively:
```bash
cd src/Fexa.ApiClient.Console
dotnet run
```

Select the appropriate menu option to test:
- Option 2: Test Visit Service
- Option 3: Test Work Order Service
- Option 4: Debug Work Order API

### Contact Support

If you encounter issues:
1. Check the filter format matches the endpoint requirements
2. Verify property names are correct for the endpoint
3. Use the debug tools to test raw API calls
4. Check API documentation at https://aafmapisandbox.fexa.io/fexa_docs/index.html