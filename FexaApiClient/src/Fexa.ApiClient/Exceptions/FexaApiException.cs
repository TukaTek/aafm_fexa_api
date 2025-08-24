using System.Net;

namespace Fexa.ApiClient.Exceptions;

public class FexaApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ResponseContent { get; }
    public string? RequestId { get; }
    
    public FexaApiException(string message) : base(message)
    {
    }
    
    public FexaApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
    
    public FexaApiException(
        string message, 
        HttpStatusCode statusCode, 
        string? responseContent = null,
        string? requestId = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        RequestId = requestId;
    }
}

public class FexaAuthenticationException : FexaApiException
{
    public FexaAuthenticationException(string message) : base(message)
    {
    }
    
    public FexaAuthenticationException(string message, HttpStatusCode statusCode, string? responseContent = null) 
        : base(message, statusCode, responseContent)
    {
    }
}

public class FexaRateLimitException : FexaApiException
{
    public int? RetryAfterSeconds { get; }
    
    public FexaRateLimitException(string message, int? retryAfterSeconds = null) 
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

public class FexaValidationException : FexaApiException
{
    public Dictionary<string, string[]>? ValidationErrors { get; }
    
    public FexaValidationException(string message, Dictionary<string, string[]>? validationErrors = null) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}