using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;
using ToDo_List.Dtos;
using ToDo_List.Models;

namespace ToDo_List.Controllers;

[ApiController]
[Route("api/list-items/{listItemId:int}/files")]
public class ListItemFilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ListItemFilesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET: api/list-items/{id}/files
    /// Returns metadata for files attached to a list item (no file bytes).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ListItemFileDto>>> GetFiles(int listItemId)
    {
        if (listItemId <= 0)
            return BadRequest(new { error = "Invalid ListItem id." });

        var exists = await _db.ListItems.AnyAsync(i => i.ListItemId == listItemId);
        if (!exists)
            return NotFound(new { error = $"ListItem {listItemId} not found." });

        var files = await _db.ListItemFiles
            .Where(f => f.ListItemId == listItemId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new ListItemFileDto
            {
                FileId = f.FileId,
                FileName = f.FileName,
                ContentType = f.ContentType ?? "",
                SizeBytes = f.FileSize,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(files);
    }

    /// <summary>
    /// POST: api/list-items/{id}/files
    /// Accepts multipart/form-data and stores file bytes in the database.
    /// Request size limited to 20 MB via attribute.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<ActionResult<List<ListItemFileDto>>> UploadFiles(int listItemId)
    {
        if (listItemId <= 0)
            return BadRequest(new { error = "Invalid ListItem id." });

        var listItem = await _db.ListItems.FindAsync(listItemId);
        if (listItem == null)
            return NotFound(new { error = $"ListItem {listItemId} not found." });

        var files = Request.Form.Files;
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded." });

        var entities = new List<ListItemFile>();

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var entity = new ListItemFile
            {
                ListItemId = listItemId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileData = ms.ToArray(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ListItemFiles.Add(entity);
            entities.Add(entity);
        }

        if (!entities.Any())
            return BadRequest(new { error = "No valid files to upload." });

        await _db.SaveChangesAsync();

        var dtos = entities.Select(e => new ListItemFileDto
        {
            FileId = e.FileId,
            FileName = e.FileName,
            ContentType = e.ContentType ?? "",
            SizeBytes = e.FileSize,
            CreatedAt = e.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// GET: api/list-items/{id}/files/{fileId}
    /// Returns the file bytes for download (convenience route).
    /// </summary>
    [HttpGet("{fileId:int}")]
    public async Task<IActionResult> DownloadFileById(int listItemId, int fileId)
    {
        var file = await _db.ListItemFiles
            .FirstOrDefaultAsync(f =>
                f.FileId == fileId && f.ListItemId == listItemId);

        if (file == null)
            return NotFound(new { error = "File not found." });

        return File(
            file.FileData,
            file.ContentType ?? "application/octet-stream",
            file.FileName
        );
    }

    /// <summary>
    /// GET: api/list-items/{id}/files/{fileId}/download
    /// Kept for compatibility; same behavior as the route without /download.
    /// </summary>
    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> DownloadFile(int listItemId, int fileId)
    {
        var file = await _db.ListItemFiles
            .FirstOrDefaultAsync(f =>
                f.FileId == fileId && f.ListItemId == listItemId);

        if (file == null)
            return NotFound(new { error = "File not found." });

        return File(
            file.FileData,
            file.ContentType ?? "application/octet-stream",
            file.FileName
        );
    }

    /// <summary>
    /// DELETE: api/list-items/{id}/files/{fileId}
    /// Deletes the specified file metadata and bytes from the DB.
    /// </summary>
    [HttpDelete("{fileId:int}")]
    public async Task<IActionResult> DeleteFile(int listItemId, int fileId)
    {
        var file = await _db.ListItemFiles
            .FirstOrDefaultAsync(f =>
                f.FileId == fileId && f.ListItemId == listItemId);

        if (file == null)
            return NotFound(new { error = "File not found." });

        _db.ListItemFiles.Remove(file);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}