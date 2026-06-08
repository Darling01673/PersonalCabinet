using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class Application
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string ApplicationNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ObjectAddress { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ExtraData { get; set; }
    public string? EnergyDeviceName { get; set; }
    public string? DeviceAddress { get; set; }
    public int? RequestedPower { get; set; }
    public int? PreviousPowerKw { get; set; }
    public int? TotalPowerKw { get; set; }
    public string? ReliabilityCategory { get; set; }
    public DateTime? DesignDeadline { get; set; }
    public string? ApplicationReason { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? Phone { get; set; }
    public string? Inn { get; set; }
    public string? PassportSeries { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportWhoIssued { get; set; }
    public string? AddressRegistr { get; set; }
    public DateTime? PassportDate { get; set; }
    public long? SNILS { get; set; }
    public DateTime? DateSNILS { get; set; }
    public string? PaymentPlan { get; set; }
    public string? GuarantyingSupplier { get; set; }
    public string? OrganizationFullName { get; set; }
    public string? OrganizationShortName { get; set; }
    public string? ContactPerson { get; set; }
    public string? ApplicantType { get; set; }

    public virtual ICollection<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = new List<ApplicationStatusHistory>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}