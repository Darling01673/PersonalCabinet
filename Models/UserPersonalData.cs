using System.ComponentModel.DataAnnotations;

namespace PersonalCabinet.Models
{
    public class UserPersonalData
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? ResidenceAddress { get; set; }
        public string? Inn { get; set; }
        public string? PassportSeries { get; set; }
        public string? PassportNumber { get; set; }
        public DateTime? PassportDate { get; set; }
        public virtual User? User { get; set; }
    }
}
