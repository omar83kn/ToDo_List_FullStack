namespace ToDo_List.Models;

public class ListItem
{
    public int ListItemId { get; set; }
    public int TodoListId { get; set; }
    public int? CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDone { get; set; }
    public DateTime? DueAt { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TodoList TodoList { get; set; } = null!;
    public Category? Category { get; set; }
}
