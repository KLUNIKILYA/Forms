using Enums.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class QuestionData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestionType Type { get; set; }
        public bool ShowInTable { get; set; }
        public bool IsRequired { get; set; }
        public int TemplateId { get; set; }
        public virtual TemplateData Template { get; set; }
        public virtual List<QuestionOptionData> Options { get; set; } = new();
    }
}
