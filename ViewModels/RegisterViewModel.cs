using System.ComponentModel.DataAnnotations;

namespace PersonalCabinet.ViewModels
{
    public class RegisterViewModel
    {
        public string UserType { get; set; } = "Individual";

        public string? FullName { get; set; }
        public string? OrgFullName { get; set; }
        public string? OrgShortName { get; set; }
        public string? ContactPerson { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }

        public bool PersonalDataConsent { get; set; }
    }
}
