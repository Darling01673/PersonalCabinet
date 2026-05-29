using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class Document
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public long? DocumentTypeId { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? MimeType { get; set; }

    public long? UploadedBy { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual DocumentType? DocumentType { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
