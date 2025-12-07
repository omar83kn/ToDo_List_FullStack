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

    // GET: api/Categories
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

    // GET: api/Categories/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
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

        if (c is null) return NotFound();
        return Ok(c);
    }

    // POST: api/Categories
    // body example:
    // { "name": "Health", "colorHex": "#00FFAA" }
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryDto dto)
    {
        var c = new Category
        {
            Name = dto.Name,
            ColorHex = dto.ColorHex
            // CreatedAt ينعبي لحاله من الmodel
        };

        _db.Categories.Add(c);
        await _db.SaveChangesAsync();

        dto.CategoryId = c.CategoryId;
        dto.CreatedAt = c.CreatedAt;

        return CreatedAtAction(nameof(GetById), new { id = c.CategoryId }, dto);
    }

    // PUT: api/Categories/5
    // body example:
    // { "name": "Work Updated", "colorHex": "#123456" }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();

        c.Name = dto.Name;
        c.ColorHex = dto.ColorHex;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Categories/5
    // ملاحظة: ListItem.CategoryId رح يصير null لأنك حاطط DeleteBehavior.SetNull
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
