namespace Forms.Models.User
{
    public class UserTemplateSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsPublic { get; set; }
        public int TimesFilled { get; set; }
        public string? ImageUrl { get; set; }
    }
}
