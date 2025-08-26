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

    // ========== Simplified DTO Endpoints ==========

    /// <summary>
    /// Gets all categories as simplified DTOs with hierarchical context preserved
    /// </summary>
    [HttpGet("simplified")]
    [ProducesResponseType(typeof(CategoryHierarchyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryHierarchyResponse>> GetSimplifiedCategories()
    {
        try
        {
            _logger.LogInformation("Getting simplified work order categories");
            var response = await _categoryService.GetSimplifiedCategoriesAsync();
            
            if (response.Warnings?.Any() == true)
            {
                _logger.LogWarning("Category hierarchy has {Count} warnings", response.Warnings.Count);
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simplified work order categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets only active categories as simplified DTOs
    /// </summary>
    [HttpGet("simplified/active")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategoryDto>>> GetActiveSimplifiedCategories()
    {
        try
        {
            _logger.LogInformation("Getting active simplified work order categories");
            var categories = await _categoryService.GetActiveSimplifiedCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active simplified categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets children of a specific category as simplified DTOs
    /// </summary>
    [HttpGet("{id}/children/simplified")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategoryDto>>> GetSimplifiedChildren(int id)
    {
        try
        {
            _logger.LogInformation("Getting simplified children for category {Id}", id);
            var children = await _categoryService.GetChildrenAsync(id);
            return Ok(children);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simplified children for category {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets root categories (no parent) as simplified DTOs
    /// </summary>
    [HttpGet("simplified/roots")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategoryDto>>> GetRootSimplifiedCategories()
    {
        try
        {
            _logger.LogInformation("Getting root simplified categories");
            var roots = await _categoryService.GetRootCategoriesAsync();
            return Ok(roots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting root simplified categories");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a category by its full hierarchical path
    /// </summary>
    /// <param name="path">Full path like "Plumbing | Grease Trap"</param>
    [HttpGet("bypath")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetByPath([FromQuery] string path)
    {
        try
        {
            _logger.LogInformation("Getting category by path: {Path}", path);
            var category = await _categoryService.GetByFullPathAsync(path);
            
            if (category == null)
            {
                _logger.LogWarning("Category not found for path: {Path}", path);
                return NotFound(new { error = $"Category with path '{path}' not found" });
            }
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by path: {Path}", path);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ========== Cache Management Endpoints ==========

    /// <summary>
    /// Starts an asynchronous background cache refresh
    /// </summary>
    [HttpPost("refresh-async")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting asynchronous cache refresh");
            await _categoryService.RefreshCacheInBackgroundAsync();
            return Accepted(new 
            { 
                message = "Cache refresh started in background",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting cache refresh");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current cache status
    /// </summary>
    [HttpGet("cache-status")]
    [ProducesResponseType(typeof(CacheStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CacheStatusDto>> GetCacheStatus()
    {
        try
        {
            var status = await _categoryService.GetCacheStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Forces a synchronous cache refresh (waits for completion)
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshCacheSync()
    {
        try
        {
            _logger.LogInformation("Starting synchronous cache refresh");
            var categories = await _categoryService.RefreshCacheAsync();
            return Ok(new 
            { 
                message = "Cache refreshed successfully",
                itemCount = categories.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache");
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