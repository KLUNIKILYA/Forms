namespace Forms.Models.Templates
{
    public class TemplateEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public bool IsPublic { get; set; }

        public List<string> Tags { get; set; } = new();
        public List<int> AllowedUserIds { get; set; } = new();
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
}
