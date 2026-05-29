using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class MessageAttachment
{
    public long Id { get; set; }

    public long MessageId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Message Message { get; set; } = null!;
}
