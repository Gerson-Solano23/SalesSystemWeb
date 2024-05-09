using System;
using System.Collections.Generic;

namespace SalesSystem.Entity;

public partial class Usuario
{
    public int IdUser { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public int? IdRol { get; set; }

    public string? Password { get; set; }

    public bool? Status { get; set; }

    public DateTime? DateRegistry { get; set; }

    public virtual Rol? IdRolNavigation { get; set; }
}
