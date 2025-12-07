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

    // ---------- DTOs used by this controller ----------

    public record ItemCreateDto(string Title, int TodoListId, int? CategoryId);
    public record ItemUpdateDto(
        string Title,
        bool IsDone,
        int? CategoryId,
        DateTime? DueAt,
        int SortOrder,
        string? Notes
    );

    // small helper to map entity -> DTO that UI expects
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
            CategoryColorHex = i.Category?.ColorHex
        };
    }

    // ---------- GET: /api/ListItems/by-list/{listId} ----------
    [HttpGet("by-list/{listId:int}")]
    public async Task<ActionResult<List<ListItemDto>>> GetByList(int listId)
    {
        // make sure the list exists (optional but nice)
        bool listExists = await _db.TodoLists.AnyAsync(t => t.TodoListId == listId);
        if (!listExists)
            return NotFound($"TodoList {listId} not found.");

        var items = await _db.ListItems
            .Where(i => i.TodoListId == listId)
            .Include(i => i.Category)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.ListItemId)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    // ---------- POST: /api/ListItems ----------
    [HttpPost]
    public async Task<ActionResult<ListItemDto>> Create(ItemCreateDto dto)
    {
        // check FK exists
        var list = await _db.TodoLists.FindAsync(dto.TodoListId);
        if (list == null)
            return BadRequest($"TodoList {dto.TodoListId} does not exist.");

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

        // reload with Category for DTO
        await _db.Entry(entity).Reference(e => e.Category).LoadAsync();

        var dtoOut = ToDto(entity);

        return CreatedAtAction(
            nameof(GetByList),
            new { listId = dtoOut.TodoListId },
            dtoOut
        );
    }

    // ---------- PUT: /api/ListItems/{id} ----------
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ListItemDto>> Update(int id, ItemUpdateDto dto)
    {
        var entity = await _db.ListItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.ListItemId == id);

        if (entity == null)
            return NotFound();

        entity.Title = dto.Title.Trim();
        entity.IsDone = dto.IsDone;
        entity.CategoryId = dto.CategoryId;
        entity.DueAt = dto.DueAt;
        entity.SortOrder = dto.SortOrder;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(e => e.Category).LoadAsync();

        return ToDto(entity);
    }

    // ---------- POST: /api/ListItems/{id}/toggle ----------
    [HttpPost("{id:int}/toggle")]
    public async Task<ActionResult<ListItemDto>> Toggle(int id)
    {
        var entity = await _db.ListItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.ListItemId == id);

        if (entity == null)
            return NotFound();

        entity.IsDone = !entity.IsDone;
        await _db.SaveChangesAsync();

        return ToDto(entity);
    }

    // ---------- DELETE: /api/ListItems/{id} ----------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.ListItems.FindAsync(id);
        if (entity == null)
            return NotFound();

        _db.ListItems.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
