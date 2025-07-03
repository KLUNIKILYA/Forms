using System.ComponentModel.DataAnnotations;

namespace Forms.Models.Templates
{
    public class TemplateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Topic { get; set; }

        public bool IsPublic { get; set; } = true;

        public List<string> Tags { get; set; } = new List<string>();
        public List<int> AllowedUserIds { get; set; } = new List<int>();
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }
}
