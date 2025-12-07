namespace ToDo_List.Dtos
{
    public class TodoListDto
    {
        public int TodoListId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int PersonId { get; set; }
        public string? PersonName { get; set; }
    }
}
