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

    // Constructor - receives the application's EF Core DbContext via DI.
    public TodoListsController(AppDbContext db) => this.db = db;

    // Helper to return a consistent validation error payload (400 Bad Request).
    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    /// <summary>
    /// GET api/todolists/by-person/{personId}
    /// Returns all todo lists that belong to the specified person.
    /// Performs validation on the incoming personId and checks the person exists.
    /// </summary>
    /// <param name="personId">The id of the person whose lists should be returned.</param>
    /// <returns>200 OK with a collection of <see cref="TodoListDto"/> or 400/404 on error.</returns>
    [HttpGet("by-person/{personId:int}")]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetByPerson(int personId)
    {
        // Validate input: ids must be positive
        if (personId <= 0)
            return ValidationError("Person id must be a positive number.");

        // Ensure the person exists before querying lists (avoids returning empty for a non-existent person)
        bool personExists = await db.Persons.AnyAsync(p => p.PersonId == personId);
        if (!personExists)
            return NotFound(new { error = $"Person {personId} not found." });

        // Query lists for the person and project them into DTOs (fetches Person.FullName for convenience)
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

        // Return 200 OK with the list of DTOs
        return Ok(lists);
    }

    /// <summary>
    /// POST api/todolists
    /// Creates a new todo list from the supplied DTO.
    /// Validates the payload and ensures the referenced person exists.
    /// </summary>
    /// <param name="dto">The todo list DTO containing PersonId and Title.</param>
    /// <returns>201 Created with the created DTO, 400 Bad Request on validation failures.</returns>
    [HttpPost]
    public async Task<ActionResult<TodoListDto>> Create([FromBody] TodoListDto dto)
    {
        // Body must be present
        if (dto is null)
            return ValidationError("Request body is required.");

        // PersonId must be positive
        if (dto.PersonId <= 0)
            return ValidationError("PersonId must be a positive number.");

        // Trim and validate title
        dto.Title = dto.Title?.Trim();
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ValidationError("Title is required.");

        // Enforce maximum title length
        if (dto.Title.Length > 200)
            return ValidationError("Title must be at most 200 characters.");

        // Verify referenced person exists (prevent FK failures and confusing data)
        bool personExists = await db.Persons.AnyAsync(p => p.PersonId == dto.PersonId);
        if (!personExists)
            return BadRequest(new { error = $"Person {dto.PersonId} does not exist." });

        // Map DTO to EF entity - only set fields we control (CreatedAt populated by DB/model)
        var list = new ToDo_List.Models.TodoList
        {
            PersonId = dto.PersonId,
            Title = dto.Title
        };

        // Add and persist to the database
        db.TodoLists.Add(list);
        await db.SaveChangesAsync();

        // Populate returned DTO with generated values (id, timestamps)
        dto.TodoListId = list.TodoListId;
        dto.CreatedAt = list.CreatedAt;

        // Return 201 Created and include a location via CreatedAtAction.
        // We point to GetByPerson (which returns lists for the person) because there's no single-get-by-id endpoint.
        return CreatedAtAction(nameof(GetByPerson), new { personId = list.PersonId }, dto);
    }

    /// <summary>
    /// DELETE api/todolists/{id}
    /// Deletes a todo list by id. Validates id and returns 404 if not found.
    /// </summary>
    /// <param name="id">The id of the TodoList to delete.</param>
    /// <returns>204 No Content on success, 400/404 on error.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Validate id
        if (id <= 0)
            return ValidationError("TodoList id must be a positive number.");

        // Find the entity by primary key. FindAsync uses the context cache first.
        var e = await db.TodoLists.FindAsync(id);
        if (e == null)
            return NotFound(new { error = $"TodoList {id} not found." });

        // Remove and persist
        db.TodoLists.Remove(e);
        await db.SaveChangesAsync();

        // Successful deletion returns 204 No Content
        return NoContent();
    }
}