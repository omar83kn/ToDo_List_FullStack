namespace ToDo_List.Dtos
{
    public class PersonDto
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
