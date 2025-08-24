using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Fexa.ApiClient.Function.Configuration;

public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Title = "Fexa API Middleware",
        Version = "1.0.0",
        Description = "Azure Functions middleware for Fexa API integration providing OAuth 2.0 authenticated access to AAFM data.",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    };

    public override List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>
    {
        new OpenApiServer
        {
            Url = "http://localhost:7071/api",
            Description = "Local Development Server"
        },
        new OpenApiServer
        {
            Url = "https://{functionAppName}.azurewebsites.net/api",
            Description = "Azure Production Server",
            Variables = new Dictionary<string, OpenApiServerVariable>
            {
                {
                    "functionAppName",
                    new OpenApiServerVariable
                    {
                        Default = "your-function-app",
                        Description = "Your Azure Function App name"
                    }
                }
            }
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}