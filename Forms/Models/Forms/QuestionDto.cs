namespace Forms.Models.Forms
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public List<string> Options { get; set; } = new();
    }
}
