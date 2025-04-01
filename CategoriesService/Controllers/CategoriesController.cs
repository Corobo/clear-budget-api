using CategoriesService.Models.DTO;
using CategoriesService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CategoriesService.Controllers;

[ApiController]
[Authorize(Roles = "clear-budget")]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetMergedCategories([FromQuery] Guid userId)
    {
        var result = await _service.GetMergedCategoriesAsync(userId);
        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "clear-budget-admin")]
    public async Task<IActionResult> GetAdminCategories()
    {
        var result = await _service.GetAdminCategoriesAsync();
        return Ok(result);
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUserCategory([FromBody] CategoryCreateDTO dto)
    {
        var userId = GetUserIdFromToken();
        var result = await _service.CreateUserCategoryAsync(userId, dto);
        return CreatedAtAction(nameof(GetMergedCategories), new { userId }, result);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "clear-budget-admin")]
    public async Task<IActionResult> CreateAdminCategory([FromBody] CategoryCreateDTO dto)
    {
        var result = await _service.CreateAdminCategoryAsync(dto);
        return Created(nameof(GetAdminCategories), result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoryColor(Guid id, [FromBody] CategoryUpdateDTO dto)
    {
        var userId = GetUserIdFromToken();
        var result = await _service.UpdateCategoryColorAsync(id, userId, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUserCategory(Guid id)
    {
        var userId = GetUserIdFromToken();
        var result = await _service.DeleteUserCategoryAsync(id, userId);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "clear-budget-admin")]
    public async Task<IActionResult> DeleteAdminCategory(Guid id)
    {
        var result = await _service.DeleteAdminCategoryAsync(id);
        return result ? NoContent() : NotFound();
    }

    private Guid GetUserIdFromToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user ID in token");
    }
}
