using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;
using ToDo_List.Dtos;
using ToDo_List.Models;

namespace ToDo_List.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    // Helper to return a consistent 400 validation error payload.
    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    /// <summary>
    /// Validate optional hex color in format #RRGGBB.
    /// Empty/null is considered valid (color is optional).
    /// </summary>
    private bool IsValidHex(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return true;
        return Regex.IsMatch(color, "^#([0-9A-Fa-f]{6})$");
    }

    /// <summary>
    /// GET: api/Categories
    /// Returns all categories projected to DTOs ordered by id.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var cats = await _db.Categories
            .OrderBy(c => c.CategoryId)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                ColorHex = c.ColorHex,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(cats);
    }

    /// <summary>
    /// GET: api/Categories/{id}
    /// Returns a single category or 404 if not found.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        if (id <= 0) return ValidationError("Category id must be a positive number.");

        var c = await _db.Categories
            .Where(x => x.CategoryId == id)
            .Select(x => new CategoryDto
            {
                CategoryId = x.CategoryId,
                Name = x.Name,
                ColorHex = x.ColorHex,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (c is null) return NotFound(new { error = $"Category {id} not found." });
        return Ok(c);
    }

    /// <summary>
    /// POST: api/Categories
    /// Validates input and creates a new category.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
    {
        if (dto is null)
            return ValidationError("Request body is required.");

        dto.Name = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return ValidationError("Name is required.");

        if (dto.Name.Length > 100)
            return ValidationError("Name must be at most 100 characters.");

        if (!IsValidHex(dto.ColorHex))
            return ValidationError("ColorHex must be in format #RRGGBB (e.g. #667eea).");

        var c = new Category
        {
            Name = dto.Name,
            ColorHex = dto.ColorHex
        };

        _db.Categories.Add(c);
        await _db.SaveChangesAsync();

        dto.CategoryId = c.CategoryId;
        dto.CreatedAt = c.CreatedAt;

        return CreatedAtAction(nameof(GetById), new { id = c.CategoryId }, dto);
    }

    /// <summary>
    /// PUT: api/Categories/{id}
    /// Updates category name and optional color after validation.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (id <= 0) return ValidationError("Category id must be a positive number.");
        if (dto is null) return ValidationError("Request body is required.");

        dto.Name = dto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return ValidationError("Name is required.");

        if (dto.Name.Length > 100)
            return ValidationError("Name must be at most 100 characters.");

        if (!IsValidHex(dto.ColorHex))
            return ValidationError("ColorHex must be in format #RRGGBB (e.g. #667eea).");

        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound(new { error = $"Category {id} not found." });

        c.Name = dto.Name;
        c.ColorHex = dto.ColorHex;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/Categories/{id}
    /// Removes the category if it exists.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return ValidationError("Category id must be a positive number.");

        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound(new { error = $"Category {id} not found." });

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}