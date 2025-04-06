using CategoriesService.Models.DTO;
using CategoriesService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth.Controllers;
using System.Security.Claims;

namespace CategoriesService.Controllers;

[Route("api/[controller]")]
public class CategoriesController : AuthorizedControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetMergedCategories()
    {
        var result = await _service.GetMergedCategoriesAsync(UserId);
        return Ok(result);
    }

    [HttpGet("ids")]
    [Authorize(Roles = "clear-budget-m2m")]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _service.GetAllCategories();
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
        var result = await _service.CreateUserCategoryAsync(UserId, dto);
        return CreatedAtAction(nameof(GetMergedCategories), new { UserId }, result);
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
        var result = await _service.UpdateCategoryColorAsync(id, UserId, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUserCategory(Guid id)
    {
        var result = await _service.DeleteUserCategoryAsync(id, UserId);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "clear-budget-admin")]
    public async Task<IActionResult> DeleteAdminCategory(Guid id)
    {
        var result = await _service.DeleteAdminCategoryAsync(id);
        return result ? NoContent() : NotFound();
    }

}
