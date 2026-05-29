using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class IndividualProfile
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
