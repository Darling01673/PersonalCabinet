using System.ComponentModel.DataAnnotations;

namespace PersonalCabinet.ViewModels
{
    public class RegisterViewModel
    {
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string Password { get; set; }

        public string UserType { get; set; } = "Individual";
        public string? OrgFullName { get; set; }
        public string? OrgShortName { get; set; }
        public string? ContactPerson { get; set; }
    }
}