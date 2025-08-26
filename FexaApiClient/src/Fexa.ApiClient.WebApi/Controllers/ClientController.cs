using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(
        IClientService clientService,
        ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ClientDto>>> GetClients([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting clients page {Page}", page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var response = await _clientService.GetClientsAsync(parameters);
            
            var dtoResponse = new PagedResponse<ClientDto>
            {
                Data = response.Data?.Select(MapToDto).ToList() ?? new List<ClientDto>(),
                TotalCount = response.TotalCount,
                Page = response.Page,
                PageSize = response.PageSize,
                TotalPages = response.TotalPages
            };
            
            return Ok(dtoResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clients");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClient(int id)
    {
        try
        {
            _logger.LogInformation("Getting client {Id}", id);
            var client = await _clientService.GetClientAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(client));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<ClientDto>>> GetAllClients([FromQuery] int maxPages = 10)
    {
        try
        {
            _logger.LogInformation("Getting all clients (max pages: {MaxPages})", maxPages);
            var clients = await _clientService.GetAllClientsAsync(null, maxPages);
            var dtos = clients.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all clients");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    private ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            Company = client.DefaultGeneralAddress?.Company,
            Dba = client.DefaultBillingAddress?.Dba,
            Active = client.Active,
            IvrId = client.IvrId,
            Address1 = client.DefaultGeneralAddress?.Address1,
            City = client.DefaultGeneralAddress?.City,
            State = client.DefaultGeneralAddress?.State,
            PostalCode = client.DefaultGeneralAddress?.PostalCode,
            Phone = client.DefaultGeneralAddress?.Phone,
            Email = client.DefaultGeneralAddress?.Email,
            CmmsProg = client.CustomFieldValues?.CmmsProg,
            InvoicingMethod = client.CustomFieldValues?.InvoicingMethod,
            Taxable = client.Organization?.Taxable
        };
    }
}