using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class User
{
    public long Id { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = new List<ApplicationStatusHistory>();

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual IndividualProfile? IndividualProfile { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual OrganizationProfile? OrganizationProfile { get; set; }
    public virtual UserPersonalData? PersonalData { get; set; }
}
