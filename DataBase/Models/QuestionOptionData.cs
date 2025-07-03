using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class QuestionOptionData
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public int QuestionId { get; set; }
        public virtual QuestionData Question { get; set; }
    }
}
