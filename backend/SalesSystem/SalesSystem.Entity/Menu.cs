using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Menu
{
    public int IdMenu { get; set; }

    public string? Name { get; set; }

    public string? Icono { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<MenuRol> MenuRols { get; } = new List<MenuRol>();
}
