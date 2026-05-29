using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace PersonalCabinet.ViewModels
{
    public class CreateApplicationViewModel
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string ResidenceAddress { get; set; }

        public string Phone { get; set; }
        public string Inn { get; set; }

        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportDate { get; set; } 
        public string ApplicationReason { get; set; }

        public string EnergyDeviceName { get; set; }
        public string DeviceAddress { get; set; }

        public int RequestedPower { get; set; }

        public int PreviousPowerKw { get; set; }

        public int TotalPowerKw { get; set; }

        public string ReliabilityCategory { get; set; }
        public DateTime? DesignDeadline { get; set; }

        public List<IFormFile> Attachments { get; set; }
    }
}