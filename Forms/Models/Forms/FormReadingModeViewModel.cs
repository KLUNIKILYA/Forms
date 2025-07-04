using DataBase.Models;

namespace Forms.Models.Forms
{
    public class FormReadingModeViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsPubliclyViewable { get; set; } = false;
        public virtual ICollection<QuestionData> Questions { get; set; }
    }
}
