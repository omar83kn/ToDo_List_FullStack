namespace ToDo_List.Dtos
{
    public class ListItemDto
    {
        public int ListItemId { get; set; }
        public string Title { get; set; } = null!;
        public bool IsDone { get; set; }
        public DateTime? DueAt { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TodoListId { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColorHex { get; set; }
    }
}
