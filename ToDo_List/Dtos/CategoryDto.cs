namespace ToDo_List.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? ColorHex { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
