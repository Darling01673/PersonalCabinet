using System.ComponentModel.DataAnnotations;

namespace PersonalCabinet.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Введите новый пароль")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Подтвердите новый пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; }
    }
}