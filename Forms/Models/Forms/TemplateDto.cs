namespace Forms.Models.Forms
{
    public class TemplateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }
}
