# AAFM Fexa API Integration

A comprehensive .NET client library and middleware solution for integrating with the Fexa API (AAFM - American Association of Fleet Managers).

**Repository**: [https://github.com/TukaTek/aafm_fexa_api](https://github.com/TukaTek/aafm_fexa_api)  
**Organization**: [TukaTek](https://github.com/TukaTek)  
**Status**: Active Development (December 2024)

## Overview

This repository provides a complete solution for integrating with the AAFM Fexa API, including:

- **Core API Client Library** - Scalable .NET client with OAuth 2.0 authentication
- **Azure Functions Middleware** - RESTful API layer for web applications
- **Interactive Console Application** - Testing and debugging tools
- **Comprehensive Documentation** - API documentation and troubleshooting guides

## Repository Structure

```
aafm_fexa_api/
├── FexaApiClient/                    # Main solution
│   ├── src/
│   │   ├── Fexa.ApiClient/          # Core library
│   │   ├── Fexa.ApiClient.Console/  # Interactive console app
│   │   └── Fexa.ApiClient.Function/ # Azure Functions middleware
│   ├── tests/                       # Unit tests
│   └── *.md                        # Documentation files
├── *.json                          # Sample data files
├── CLAUDE.md                       # Development guidance
├── FEXA_API_DOCUMENTATION.md       # Comprehensive API docs
└── README.md                       # This file
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Fexa API credentials (Client ID and Secret)
- Azure Functions Core Tools (for middleware)

### Clone and Setup

```bash
git clone https://github.com/TukaTek/aafm_fexa_api.git
cd aafm_fexa_api/FexaApiClient
```

### Configure Credentials

```bash
cd src/Fexa.ApiClient.Console
dotnet user-secrets init
dotnet user-secrets set "FexaApi:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "FexaApi:ClientSecret" "YOUR_CLIENT_SECRET"
```

### Run Interactive Console

```bash
dotnet run
```

The console application provides a comprehensive menu system for testing all API features including work orders, visits, users, transitions, and more.

## Key Features

### Core API Client Library

- **OAuth 2.0 Authentication** - Automatic token management and refresh
- **Resilient HTTP Client** - Built-in retry policies and circuit breaker
- **Comprehensive Service Layer** - Work Orders, Visits, Users, Invoices, Transitions
- **Advanced Filtering** - Powerful FilterBuilder with multiple operators
- **Error Handling** - Custom exceptions for different scenarios
- **Performance Optimized** - Caching and bulk operations

### Azure Functions Middleware

- **RESTful API Layer** - HTTP endpoints for web applications
- **Health Monitoring** - Built-in health checks
- **Scalable Architecture** - Azure Functions serverless deployment
- **Security** - Function-level authentication

### Interactive Console Application

- **Comprehensive Testing** - Full API testing capabilities
- **Debug Tools** - Advanced debugging and filter testing
- **Status Management** - Interactive work order status updates
- **Data Export** - Export transitions and data to JSON

## API Endpoints Supported

- **Work Orders** - CRUD operations, status updates, filtering
- **Visits** - Scheduling, tracking, filtering by multiple criteria
- **Users** - User management and authentication
- **Transitions** - Workflow state management
- **Invoices** - Client and vendor invoice tracking
- **Regions** - Geographic region management
- **Severities** - Priority level management
- **Notes** - Comment and note management

## Documentation

### For Developers
- **[FexaApiClient/README.md](FexaApiClient/README.md)** - Core library documentation
- **[CLAUDE.md](CLAUDE.md)** - Development guidance and best practices
- **[FEXA_API_DOCUMENTATION.md](FEXA_API_DOCUMENTATION.md)** - Comprehensive API reference

### For Troubleshooting
- **[VISITS_TROUBLESHOOTING.md](FexaApiClient/VISITS_TROUBLESHOOTING.md)** - Visit and work order troubleshooting

### For Azure Functions
- **[Azure Functions README](FexaApiClient/src/Fexa.ApiClient.Function/README.md)** - Middleware documentation

## Sample Data

The repository includes sample JSON files with real API responses:
- `workorder.json` - Work order data examples
- `visits.json` - Visit scheduling examples  
- `transitions.json` - Workflow transition examples
- `subcontractor.json` - Subcontractor data examples

## Recent Updates (December 2024)

### Repository Migration
- Migrated to TukaTek organization
- Updated all documentation and URLs
- Enhanced repository structure

### API Improvements
- Fixed work order vendor filtering (`vendors.id` instead of `assigned_to`)
- Fixed client filtering (`clients.id` instead of `placed_for`)
- Improved filter format handling for URL-encoded JSON arrays
- Enhanced error detection and handling

### Documentation Updates
- Comprehensive documentation refresh
- Updated all README files with TukaTek information
- Enhanced troubleshooting guides
- Added repository information sections

## Getting Help

### GitHub Issues
Create an issue for:
- Bug reports
- Feature requests
- Integration questions
- Documentation improvements

**Issues**: [https://github.com/TukaTek/aafm_fexa_api/issues](https://github.com/TukaTek/aafm_fexa_api/issues)

### Contact
- **Organization**: [TukaTek](https://github.com/TukaTek)
- **Repository**: [aafm_fexa_api](https://github.com/TukaTek/aafm_fexa_api)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Follow existing code patterns and conventions
4. Add tests for new functionality
5. Update documentation as needed
6. Test with the interactive console application
7. Submit a pull request

## License

This project is developed by [TukaTek](https://github.com/TukaTek) for integration with the AAFM Fexa API.

## External Resources

- **Fexa API Documentation**: [https://aafmapisandbox.fexa.io/fexa_docs/index.html](https://aafmapisandbox.fexa.io/fexa_docs/index.html)
- **AAFM**: American Association of Fleet Managers
- **TukaTek Organization**: [https://github.com/TukaTek](https://github.com/TukaTek)

---

**Latest Update**: December 2024  
**Maintained by**: [TukaTek](https://github.com/TukaTek) development team
