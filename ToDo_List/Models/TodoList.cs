namespace ToDo_List.Models;

public class TodoList
{
    public int TodoListId { get; set; }
    public int PersonId { get; set; }
    public string Title { get; set; } = "New list";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Person Person { get; set; } = null!;
    public ICollection<ListItem> Items { get; set; } = new List<ListItem>();
}
