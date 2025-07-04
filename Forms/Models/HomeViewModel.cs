using Forms.Models.Templates;

namespace Forms.Models
{
    public class HomeViewModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }

        public List<TemplateIndexViewModel> PopularTemplates { get; set; }
        public List<TemplateIndexViewModel> RecentUserTemplates { get; set; } = new();

        public HomeViewModel()
        {
            PopularTemplates = new List<TemplateIndexViewModel>();
        }
    }
}
