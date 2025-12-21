using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;
using ToDo_List.Dtos;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly AppDbContext db;
    public PersonsController(AppDbContext db) => this.db = db;

    // Helper to return a consistent 400 validation error payload.
    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    /// <summary>
    /// Lightweight email format validation. Returns false for null/empty values.
    /// Note: simple regex, not a full RFC validator.
    /// </summary>
    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^\S+@\S+\.\S+$");
    }

    /// <summary>
    /// GET: api/persons
    /// Returns all persons projected to PersonDto.
    /// Uses projection to fetch only required fields.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonDto>>> Get()
    {
        var data = await db.Persons
            .Select(p => new PersonDto
            {
                PersonId = p.PersonId,
                FullName = p.FullName,
                Email = p.Email,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    /// <summary>
    /// POST: api/persons
    /// Creates a new Person after validating FullName and Email.
    /// Returns 201 Created with the created DTO.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PersonDto>> Create([FromBody] PersonDto dto)
    {
        if (dto is null)
            return ValidationError("Request body is required.");

        // Normalize inputs
        dto.FullName = dto.FullName?.Trim();
        dto.Email = dto.Email?.Trim();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ValidationError("FullName is required.");

        if (dto.FullName.Length > 100)
            return ValidationError("FullName must be at most 100 characters.");

        if (!IsValidEmail(dto.Email))
            return ValidationError("Email is not valid.");

        var p = new ToDo_List.Models.Person
        {
            FullName = dto.FullName,
            Email = dto.Email
        };

        db.Persons.Add(p);
        await db.SaveChangesAsync();

        // Return generated values
        dto.PersonId = p.PersonId;
        dto.CreatedAt = p.CreatedAt;

        // CreatedAtAction points to the list endpoint since no single-get is defined.
        return CreatedAtAction(nameof(Get), new { id = p.PersonId }, dto);
    }
}