using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class UsuarioDTO
    {
        public int IdUser { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public int? IdRol { get; set; }
        public string? RolDescription { get; set; }

        public string? Password { get; set; }

        public byte? Status { get; set; }
    }
}
