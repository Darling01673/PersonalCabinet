using System;
using System.Collections.Generic;

namespace PersonalCabinet.Models;

public partial class DocumentType
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
