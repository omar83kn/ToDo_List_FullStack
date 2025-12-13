using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;
using ToDo_List.Dtos;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoListsController : ControllerBase
{
    private readonly AppDbContext db;

    public TodoListsController(AppDbContext db) => this.db = db;

    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    [HttpGet("by-person/{personId:int}")]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetByPerson(int personId)
    {
        if (personId <= 0)
            return ValidationError("Person id must be a positive number.");

        bool personExists = await db.Persons.AnyAsync(p => p.PersonId == personId);
        if (!personExists)
            return NotFound(new { error = $"Person {personId} not found." });

        var lists = await db.TodoLists
            .Where(t => t.PersonId == personId)
            .Select(t => new TodoListDto
            {
                TodoListId = t.TodoListId,
                PersonId = t.PersonId,
                Title = t.Title,
                CreatedAt = t.CreatedAt,
                PersonName = t.Person.FullName
            })
            .ToListAsync();

        return Ok(lists);
    }

    [HttpPost]
    public async Task<ActionResult<TodoListDto>> Create([FromBody] TodoListDto dto)
    {
        if (dto is null)
            return ValidationError("Request body is required.");

        if (dto.PersonId <= 0)
            return ValidationError("PersonId must be a positive number.");

        dto.Title = dto.Title?.Trim();
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ValidationError("Title is required.");

        if (dto.Title.Length > 200)
            return ValidationError("Title must be at most 200 characters.");

        bool personExists = await db.Persons.AnyAsync(p => p.PersonId == dto.PersonId);
        if (!personExists)
            return BadRequest(new { error = $"Person {dto.PersonId} does not exist." });

        var list = new ToDo_List.Models.TodoList
        {
            PersonId = dto.PersonId,
            Title = dto.Title
        };

        db.TodoLists.Add(list);
        await db.SaveChangesAsync();

        dto.TodoListId = list.TodoListId;
        dto.CreatedAt = list.CreatedAt;

        return CreatedAtAction(nameof(GetByPerson), new { personId = list.PersonId }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return ValidationError("TodoList id must be a positive number.");

        var e = await db.TodoLists.FindAsync(id);
        if (e == null)
            return NotFound(new { error = $"TodoList {id} not found." });

        db.TodoLists.Remove(e);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
