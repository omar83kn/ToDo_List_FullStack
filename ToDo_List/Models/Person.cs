namespace ToDo_List.Models;

public class Person
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TodoList> TodoLists { get; set; } = new List<TodoList>();
}
