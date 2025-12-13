namespace ToDo_List.Dtos
{
    public class ListItemFileDto
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
