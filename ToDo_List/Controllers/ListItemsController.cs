using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;
using ToDo_List.Dtos;
using ToDo_List.Models;

namespace ToDo_List.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ListItemsController(AppDbContext db)
    {
        _db = db;
    }

    // Local DTOs for create/update payloads (kept inside controller for clarity).
    public record ItemCreateDto(string Title, int TodoListId, int? CategoryId);
    public record ItemUpdateDto(
        string Title,
        bool IsDone,
        int? CategoryId,
        DateTime? DueAt,
        int SortOrder,
        string? Notes
    );

    // Helper to return a consistent 400 validation error payload.
    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    /// <summary>
    /// Map ListItem entity to ListItemDto (includes Category and Files).
    /// </summary>
    private static ListItemDto ToDto(ListItem i)
    {
        return new ListItemDto
        {
            ListItemId = i.ListItemId,
            Title = i.Title,
            IsDone = i.IsDone,
            DueAt = i.DueAt,
            SortOrder = i.SortOrder,
            Notes = i.Notes,
            CreatedAt = i.CreatedAt,
            TodoListId = i.TodoListId,
            CategoryId = i.CategoryId,
            CategoryName = i.Category?.Name,
            CategoryColorHex = i.Category?.ColorHex,
            Files = i.Files
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new ListItemFileDto
                {
                    FileId = f.FileId,
                    FileName = f.FileName,
                    ContentType = f.ContentType ?? "",
                    SizeBytes = f.FileSize,
                    CreatedAt = f.CreatedAt
                })
                .ToList()
        };
    }

    /// <summary>
    /// GET: /api/ListItems/by-list/{listId}
    /// Returns all items for a todo list. Includes Category and Files.
    /// </summary>
    [HttpGet("by-list/{listId:int}")]
    public async Task<ActionResult<List<ListItemDto>>> GetByList(int listId)
    {
        if (listId <= 0)
            return ValidationError("TodoList id must be a positive number.");

        bool listExists = await _db.TodoLists.AnyAsync(t => t.TodoListId == listId);
        if (!listExists)
            return NotFound(new { error = $"TodoList {listId} not found." });

        var items = await _db.ListItems
            .Where(i => i.TodoListId == listId)
            .Include(i => i.Category)
            .Include(i => i.Files)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.ListItemId)
            .ToListAsync();

        return Ok(items.Select(ToDto).ToList());
    }

    /// <summary>
    /// POST: /api/ListItems
    /// Creates a new ListItem after validating title, list and optional category.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ListItemDto>> Create([FromBody] ItemCreateDto dto)
    {
        if (dto is null)
            return ValidationError("Request body is required.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ValidationError("Title is required.");

        if (dto.Title.Length > 200)
            return ValidationError("Title must be at most 200 characters.");

        if (dto.TodoListId <= 0)
            return ValidationError("TodoListId must be a positive number.");

        var list = await _db.TodoLists.FindAsync(dto.TodoListId);
        if (list == null)
            return BadRequest(new { error = $"TodoList {dto.TodoListId} does not exist." });

        if (dto.CategoryId.HasValue)
        {
            bool catExists = await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId.Value);
            if (!catExists)
                return BadRequest(new { error = $"Category {dto.CategoryId.Value} does not exist." });
        }

        var entity = new ListItem
        {
            Title = dto.Title.Trim(),
            TodoListId = dto.TodoListId,
            CategoryId = dto.CategoryId,
            IsDone = false,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.ListItems.Add(entity);
        await _db.SaveChangesAsync();

        // Load related navigation properties for response (Category + Files)
        await _db.Entry(entity).Reference(e => e.Category).LoadAsync();
        await _db.Entry(entity).Collection(e => e.Files).LoadAsync();

        var dtoOut = ToDto(entity);

        return CreatedAtAction(
            nameof(GetByList),
            new { listId = dtoOut.TodoListId },
            dtoOut
        );
    }

    /// <summary>
    /// PUT: /api/ListItems/{id}
    /// Updates an existing ListItem and returns the updated DTO.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ListItemDto>> Update(int id, [FromBody] ItemUpdateDto dto)
    {
        if (id <= 0) return ValidationError("ListItem id must be a positive number.");
        if (dto is null) return ValidationError("Request body is required.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ValidationError("Title is required.");

        if (dto.Title.Length > 200)
            return ValidationError("Title must be at most 200 characters.");

        if (dto.CategoryId.HasValue)
        {
            bool catExists = await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId.Value);
            if (!catExists)
                return BadRequest(new { error = $"Category {dto.CategoryId.Value} does not exist." });
        }

        var entity = await _db.ListItems
            .Include(i => i.Category)
            .Include(i => i.Files)
            .FirstOrDefaultAsync(i => i.ListItemId == id);

        if (entity == null)
            return NotFound(new { error = $"ListItem {id} not found." });

        // Apply updates
        entity.Title = dto.Title.Trim();
        entity.IsDone = dto.IsDone;
        entity.CategoryId = dto.CategoryId;
        entity.DueAt = dto.DueAt;
        entity.SortOrder = dto.SortOrder;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync();

        // Ensure navigation properties are fresh
        await _db.Entry(entity).Reference(e => e.Category).LoadAsync();
        await _db.Entry(entity).Collection(e => e.Files).LoadAsync();

        return Ok(ToDto(entity));
    }

    /// <summary>
    /// POST: /api/ListItems/{id}/toggle
    /// Toggle the IsDone flag for the item and return the updated DTO.
    /// </summary>
    [HttpPost("{id:int}/toggle")]
    public async Task<ActionResult<ListItemDto>> Toggle(int id)
    {
        if (id <= 0) return ValidationError("ListItem id must be a positive number.");

        var entity = await _db.ListItems
            .Include(i => i.Category)
            .Include(i => i.Files)
            .FirstOrDefaultAsync(i => i.ListItemId == id);

        if (entity == null)
            return NotFound(new { error = $"ListItem {id} not found." });

        entity.IsDone = !entity.IsDone;
        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    /// <summary>
    /// DELETE: /api/ListItems/{id}
    /// Deletes the list item. Files are removed via cascade if configured.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return ValidationError("ListItem id must be a positive number.");

        var entity = await _db.ListItems.FindAsync(id);
        if (entity == null)
            return NotFound(new { error = $"ListItem {id} not found." });

        _db.ListItems.Remove(entity); // files will be deleted by cascade if configured
        await _db.SaveChangesAsync();
        return NoContent();
    }
}