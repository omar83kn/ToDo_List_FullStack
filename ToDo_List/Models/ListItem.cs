using System.ComponentModel.DataAnnotations;

namespace ToDo_List.Models;

public class ListItem
{
    public int ListItemId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public bool IsDone { get; set; }
    public DateTime? DueAt { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int TodoListId { get; set; }
    public TodoList? TodoList { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // ✅ New: multiple files
    public ICollection<ListItemFile> Files { get; set; } = new List<ListItemFile>();
}
