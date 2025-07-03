using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class AnswerData
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool? BoolValue { get; set; }

        public int FormId { get; set; }
        public virtual FormData Form { get; set; }
        public int QuestionId { get; set; }
        public virtual QuestionData Question { get; set; }
    }
}
