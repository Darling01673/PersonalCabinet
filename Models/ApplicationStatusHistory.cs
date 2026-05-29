using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class ApplicationStatusHistory
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public long? ChangedBy { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual User? ChangedByNavigation { get; set; }
}
