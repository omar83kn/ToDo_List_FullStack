using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDo_List.Models;

public class ListItemFile
{
    [Key]
    public int FileId { get; set; }

    [Required]
    public int ListItemId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = "";

    [MaxLength(100)]
    public string? ContentType { get; set; }   // image/png, application/pdf

    public long FileSize { get; set; }         // bytes

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ListItem? ListItem { get; set; }
}
