using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderCategoryController : ControllerBase
{
    private readonly IWorkOrderCategoryService _categoryService;
    private readonly ILogger<WorkOrderCategoryController> _logger;

    public WorkOrderCategoryController(
        IWorkOrderCategoryService categoryService,
        ILogger<WorkOrderCategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkOrderCategoryDto>>> GetAllCategories()
    {
        try
        {
            _logger.LogInformation("Getting all work order categories");
            var categories = await _categoryService.GetAllCategoriesAsync();
            var dtos = categories.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all work order categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<WorkOrderCategoryDto>>> GetActiveCategories()
    {
        try
        {
            _logger.LogInformation("Getting active work order categories");
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var dtos = categories.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active work order categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("leaf")]
    public async Task<ActionResult<List<WorkOrderCategoryDto>>> GetLeafCategories()
    {
        try
        {
            _logger.LogInformation("Getting leaf work order categories");
            var categories = await _categoryService.GetLeafCategoriesAsync();
            var dtos = categories.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaf work order categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("parent")]
    public async Task<ActionResult<List<WorkOrderCategoryDto>>> GetParentCategories()
    {
        try
        {
            _logger.LogInformation("Getting parent work order categories");
            var categories = await _categoryService.GetParentCategoriesAsync();
            var dtos = categories.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent work order categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrderCategoryDto>> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("Getting work order category {Id}", id);
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order category {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("byname/{name}")]
    public async Task<ActionResult<WorkOrderCategoryDto>> GetCategoryByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting work order category by name {Name}", name);
            var category = await _categoryService.GetCategoryByNameAsync(name);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order category by name {Name}", name);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private WorkOrderCategoryDto MapToDto(WorkOrderCategory category)
    {
        return new WorkOrderCategoryDto
        {
            Id = category.Id,
            Category = category.Category,
            Description = category.Description,
            ParentCategory = category.Parent?.Category ?? category.FirstAncestorDisplay,
            FullPath = category.CategoryWithFirstAncestor ?? category.Category
        };
    }
}