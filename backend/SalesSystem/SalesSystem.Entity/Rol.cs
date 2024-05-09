using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Rol
{
    public int IdRol { get; set; }

    public string? Name { get; set; }

    public DateTime? DateRegistry { get; set; }

    public virtual ICollection<MenuRol> MenuRols { get; } = new List<MenuRol>();

    public virtual ICollection<Usuario> Usuarios { get; } = new List<Usuario>();
}
