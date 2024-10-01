using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Product
{
    public int IdProduct { get; set; }

    public string? Name { get; set; }

    public int? IdCategory { get; set; }

    public int? Stock { get; set; }

    public decimal? Price { get; set; }

    public bool? Status { get; set; }

    public DateTime? DateRegistry { get; set; }

    public string? img { get; set; }

    public virtual Category? IdCategoryNavigation { get; set; }

    public virtual ICollection<SaleDetail> SaleDetails { get; } = new List<SaleDetail>();
}
