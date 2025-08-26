using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly ILogger<VendorController> _logger;

    public VendorController(
        IVendorService vendorService,
        ILogger<VendorController> logger)
    {
        _vendorService = vendorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Vendor>>> GetVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting vendors page {Page}", page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var vendors = await _vendorService.GetVendorsAsync(parameters);
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vendor>> GetVendor(int id)
    {
        try
        {
            _logger.LogInformation("Getting vendor {Id}", id);
            var vendor = await _vendorService.GetVendorAsync(id);
            return Ok(vendor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("workorder/{workOrderId}")]
    public async Task<ActionResult<List<Vendor>>> GetVendorsByWorkOrder(int workOrderId)
    {
        try
        {
            _logger.LogInformation("Getting vendors for work order {WorkOrderId}", workOrderId);
            var vendors = await _vendorService.GetVendorsByWorkOrderIdAsync(workOrderId);
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<PagedResponse<Vendor>>> GetActiveVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting active vendors page {Page}", page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var vendors = await _vendorService.GetActiveVendorsAsync(parameters);
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vendors");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("assignable")]
    public async Task<ActionResult<PagedResponse<Vendor>>> GetAssignableVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting assignable vendors page {Page}", page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var vendors = await _vendorService.GetAssignableVendorsAsync(parameters);
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable vendors");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("compliance")]
    public async Task<ActionResult<PagedResponse<Vendor>>> GetVendorsByCompliance(
        [FromQuery] bool isCompliant,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting vendors by compliance {IsCompliant} page {Page}", isCompliant, page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var vendors = await _vendorService.GetVendorsByComplianceStatusAsync(isCompliant, parameters);
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors by compliance");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}