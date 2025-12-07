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
    public async Task<ActionResult<PersonDto>> Create(PersonDto dto)
    {
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
