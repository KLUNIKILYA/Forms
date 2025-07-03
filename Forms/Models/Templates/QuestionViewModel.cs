using Enums.Question;
using System.ComponentModel.DataAnnotations;

namespace Forms.Models.Templates
{
    public class QuestionViewModel
    {
        [Required]
        public string Text { get; set; }
        public string Description { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        public bool IsRequired { get; set; }
        public bool ShowInTable { get; set; }
        public List<string> Options { get; set; } = new List<string>();
    }
}
