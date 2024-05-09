using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Sale
{
    public int IdSale { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? PaymentType { get; set; }

    public decimal? Total { get; set; }

    public DateTime? DateRegistry { get; set; }

    public virtual ICollection<SaleDetail> SaleDetails { get; } = new List<SaleDetail>();
}
