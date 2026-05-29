using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class OrganizationProfile
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? ShortName { get; set; }

    public string? ContactPerson { get; set; }

    public virtual User User { get; set; } = null!;
}
