using System.ComponentModel.DataAnnotations;
namespace PersonalCabinet.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
