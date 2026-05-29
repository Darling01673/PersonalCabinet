using System.ComponentModel.DataAnnotations;

namespace PersonalCabinet.ViewModels
{
    public class ProfileViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; }

        public string UserType { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string OrgFullName { get; set; }
        public string OrgShortName { get; set; }
        public string ContactPerson { get; set; }

        public string ResidenceAddress { get; set; }
        public string Inn { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportDate { get; set; }
    }
}