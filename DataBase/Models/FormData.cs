using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class FormData
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int TemplateId { get; set; }
        public virtual TemplateData Template { get; set; }
        public int UserId { get; set; }
        public virtual UserData User { get; set; }
        public virtual List<AnswerData> Answers { get; set; } = new();
    }
}
