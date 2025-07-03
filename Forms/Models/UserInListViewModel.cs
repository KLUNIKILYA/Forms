namespace Forms.Models
{
    public class UserInListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegistrationTime { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSelected { get; set; }
        public int[] ActivityData { get; set; }

        public string LastSeenFormatted
        {
            get
            {
                var timeDiff = DateTime.Now - LastLoginTime;

                if (timeDiff.TotalMinutes < 1)
                    return "less than a minute ago";
                if (timeDiff.TotalMinutes < 60)
                    return $"{(int)timeDiff.TotalMinutes} minutes ago";
                if (timeDiff.TotalHours < 24)
                    return $"{(int)timeDiff.TotalHours} hours ago";
                if (timeDiff.TotalDays < 7)
                    return $"{(int)timeDiff.TotalDays} days ago";
                if (timeDiff.TotalDays < 30)
                    return $"{(int)(timeDiff.TotalDays / 7)} weeks ago";

                return LastLoginTime.ToString("MMM d, yyyy");
            }
        }
    }
}
