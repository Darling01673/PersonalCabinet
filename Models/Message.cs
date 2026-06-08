using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class Message
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public long SenderId { get; set; }

    public string Message1 { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
