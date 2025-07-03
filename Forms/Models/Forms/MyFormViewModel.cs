namespace Forms.Models.Forms
{
    public class MyFormViewModel
    {
        public int FormId { get; set; }
        public string TemplateTitle { get; set; }
        public string TemplateDescription { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? TemplateImageUrl { get; set; }
    }
}
