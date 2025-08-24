using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.Http;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFexaApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<FexaApiOptions>(configuration.GetSection(FexaApiOptions.SectionName));
        
        // Add options validation
        services.AddSingleton<IValidateOptions<FexaApiOptions>, FexaApiOptionsValidator>();
        
        // Register token service with named HttpClient
        services.AddHttpClient("TokenService", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        services.AddSingleton<ITokenService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("TokenService");
            var logger = serviceProvider.GetRequiredService<ILogger<TokenService>>();
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>();
            return new TokenService(httpClient, logger, options);
        });
        
        // Register authentication handler
        services.AddTransient<FexaAuthenticationHandler>();
        
        // Configure HttpClient with Polly retry policies
        services.AddHttpClient<IFexaApiService, FexaApiService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>().Value;
            options.Validate();
            
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "FexaApiClient/1.0");
        })
        .AddHttpMessageHandler<FexaAuthenticationHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        // Register additional services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IClientInvoiceService, ClientInvoiceService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<ITransitionService, TransitionService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ISeverityService, SeverityService>();
        
        return services;
    }
    
    public static IServiceCollection AddFexaApiClient(
        this IServiceCollection services,
        Action<FexaApiOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        // Add options validation
        services.AddSingleton<IValidateOptions<FexaApiOptions>, FexaApiOptionsValidator>();
        
        // Register token service with named HttpClient
        services.AddHttpClient("TokenService", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        services.AddSingleton<ITokenService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("TokenService");
            var logger = serviceProvider.GetRequiredService<ILogger<TokenService>>();
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>();
            return new TokenService(httpClient, logger, options);
        });
        
        // Register authentication handler
        services.AddTransient<FexaAuthenticationHandler>();
        
        // Configure HttpClient
        services.AddHttpClient<IFexaApiService, FexaApiService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FexaApiOptions>>().Value;
            options.Validate();
            
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "FexaApiClient/1.0");
        })
        .AddHttpMessageHandler<FexaAuthenticationHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        // Register additional services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IClientInvoiceService, ClientInvoiceService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<ITransitionService, TransitionService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ISeverityService, SeverityService>();
        
        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Retry logic - can be extended to log if needed
                });
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, timespan) =>
                {
                    // Circuit breaker opened
                },
                onReset: () =>
                {
                    // Circuit breaker closed
                });
    }
}

public class FexaApiOptionsValidator : IValidateOptions<FexaApiOptions>
{
    public ValidateOptionsResult Validate(string? name, FexaApiOptions options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (ArgumentException ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}