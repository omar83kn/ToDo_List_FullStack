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

    [HttpGet("by-person/{personId:int}")]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetByPerson(int personId)
    {
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
    public async Task<ActionResult<TodoListDto>> Create(TodoListDto dto)
    {
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
        var e = await db.TodoLists.FindAsync(id);
        if (e == null) return NotFound();

        db.TodoLists.Remove(e);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
