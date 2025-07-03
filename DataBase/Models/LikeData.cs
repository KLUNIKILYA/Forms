using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class LikeData
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }


        public int TemplateId { get; set; }
        public virtual TemplateData Template { get; set; }
        public int UserId { get; set; }
        public virtual UserData User { get; set; }
    }
}
