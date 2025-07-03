using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enums.User;

namespace DataBase.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsBlocked { get; set; }
        public Role Role { get; set; }

        public List<TemplateData> Templates { get; set; } = new();
        public List<FormData> Forms { get; set; } = new();
        public List<CommentData> Comments { get; set; } = new();
        public List<LikeData> Likes { get; set; } = new();
        public List<TemplateAccess> AccessTemplates { get; set; } = new();
    }
}
