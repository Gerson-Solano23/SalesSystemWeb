using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Category
{
    public int IdCategory { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public DateTime? DateRegistry { get; set; }

    public virtual ICollection<Product> Products { get; } = new List<Product>();
}
