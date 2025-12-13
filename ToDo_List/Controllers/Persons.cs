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

    private ActionResult ValidationError(string message)
        => BadRequest(new { error = message });

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^\S+@\S+\.\S+$");
    }

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

    [HttpPost]
    public async Task<ActionResult<PersonDto>> Create([FromBody] PersonDto dto)
    {
        if (dto is null)
            return ValidationError("Request body is required.");

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

        dto.PersonId = p.PersonId;
        dto.CreatedAt = p.CreatedAt;

        return CreatedAtAction(nameof(Get), new { id = p.PersonId }, dto);
    }
}
