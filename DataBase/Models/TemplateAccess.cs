using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class TemplateAccess
    {
        public int TemplateId { get; set; }
        public TemplateData Template { get; set; }
        public int UserId { get; set; }
        public UserData User { get; set; }
    }
}
