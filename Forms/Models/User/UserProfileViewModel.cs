using Forms.Models.Forms;

namespace Forms.Models.User
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }

        public List<UserTemplateSummaryViewModel> Templates { get; set; } = new();
        public List<MyFormViewModel> FilledForms { get; set; } = new();
    }
}
