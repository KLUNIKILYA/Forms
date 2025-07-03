namespace Forms.Models
{
    public class UserManagementViewModel
    {
        public List<UserInListViewModel> Users { get; set; }
        public string SortField { get; set; }
        public bool AllSelected { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalUsers { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalUsers / (double)PageSize);
    }
}
