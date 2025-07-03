using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class TagData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<TemplateData> TemplateTags { get; set; } = new();
    }
}
