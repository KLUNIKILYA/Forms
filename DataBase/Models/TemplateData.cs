using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class TemplateData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public int AuthorId { get; set; }
        public UserData Author { get; set; }

        public virtual List<TagData> Tags { get; set; } = new();
        public virtual List<QuestionData> Questions { get; set; } = new();
        public virtual List<FormData> Forms { get; set; } = new();
        public virtual List<TemplateAccess> AllowedUsers { get; set; } = new();
        public virtual List<CommentData> Comments { get; set; } = new();
        public virtual List<LikeData> Likes { get; set; } = new();
    }
}
