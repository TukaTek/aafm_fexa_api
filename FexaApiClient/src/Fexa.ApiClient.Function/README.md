# Fexa API Middleware - Azure Functions

This Azure Functions project provides a middleware API layer for the Fexa API client library.

## Configuration

Before running the functions, configure your Fexa API credentials:

### For Local Development
Update `local.settings.json`:
```json
{
  "Values": {
    "FexaApi__ClientId": "YOUR_CLIENT_ID",
    "FexaApi__ClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

### For Production (Azure)
Set these environment variables in your Azure Function App:
- `FexaApi__ClientId`
- `FexaApi__ClientSecret`
- `FexaApi__BaseUrl` (optional, defaults to sandbox)

## Running Locally

```bash
cd src/Fexa.ApiClient.Function
func start
```

Note: You need Azure Functions Core Tools installed. Install it with:
```bash
npm install -g azure-functions-core-tools@4
```

## API Endpoints

### Health Check
- `GET /api/health` - Basic health check (anonymous)
- `GET /api/health/detailed` - Detailed health check with Fexa API connectivity status

### Work Orders
- `GET /api/workorders/vendor/{vendorId}` - Get work orders by vendor
  - Query params: `clientId`, `status`
- `GET /api/workorders/{id}` - Get specific work order
- `PUT /api/workorders/{id}/status/{statusId}` - Update work order status
- `GET /api/workorders/{id}/transitions` - Get available status transitions

### Visits
- `GET /api/visits/workorder/{workOrderId}` - Get visits by work order
- `POST /api/visits/daterange` - Get visits by date range
  - Body: `{ "startDate": "2025-08-14", "endDate": "2025-08-15", "workOrderId": 123 }`
- `GET /api/visits/{id}` - Get specific visit

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get specific user

## Example Requests

### Get Work Orders for Vendor
```bash
curl http://localhost:7071/api/workorders/vendor/443210 \
  -H "x-functions-key: YOUR_FUNCTION_KEY"
```

### Update Work Order Status
```bash
curl -X PUT http://localhost:7071/api/workorders/12345/status/2 \
  -H "x-functions-key: YOUR_FUNCTION_KEY"
```

### Get Visits by Date Range
```bash
curl -X POST http://localhost:7071/api/visits/daterange \
  -H "Content-Type: application/json" \
  -H "x-functions-key: YOUR_FUNCTION_KEY" \
  -d '{"startDate":"2025-08-14","endDate":"2025-08-15"}'
```

## Deployment to Azure

1. Create an Azure Function App (Linux, .NET 8 Isolated)
2. Configure application settings with your Fexa API credentials
3. Deploy using Azure CLI:
   ```bash
   func azure functionapp publish YOUR_FUNCTION_APP_NAME
   ```

Or using Visual Studio Code:
1. Install Azure Functions extension
2. Right-click on the Function project
3. Select "Deploy to Function App..."

## Security

- All endpoints except `/api/health` require function-level authentication
- Use API keys or Azure AD authentication in production
- Store credentials securely using Azure Key Vault for production deployments

## Extending the API

To add new endpoints:
1. Create a new class in the `Functions` folder
2. Inject required Fexa services via constructor
3. Add `[Function]` decorated methods for each endpoint
4. Follow the existing pattern for error handling and response formatting