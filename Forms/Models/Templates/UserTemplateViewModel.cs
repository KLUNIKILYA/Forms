namespace Forms.Models.Templates
{
    public class UserTemplateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
        public int TimesFilled { get; set; }
    }
}
