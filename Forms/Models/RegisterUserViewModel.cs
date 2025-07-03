using System.ComponentModel.DataAnnotations;

namespace Forms.Models
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Введите свое имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пароль должен содержать хотя бы один символ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string Email { get; set; } 
    }
}
