namespace ToDo_List.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? ColorHex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ListItem> Items { get; set; } = new List<ListItem>();
}
