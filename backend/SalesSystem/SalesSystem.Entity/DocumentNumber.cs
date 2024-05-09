using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class DocumentNumber
{
    public int IdDocumentNumber { get; set; }

    public int LastNumber { get; set; }

    public DateTime? DateRegistry { get; set; }
}
