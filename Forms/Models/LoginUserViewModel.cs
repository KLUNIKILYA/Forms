using System.ComponentModel.DataAnnotations;

namespace Forms.Models
{
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string Email { get; set; }
    }
}
