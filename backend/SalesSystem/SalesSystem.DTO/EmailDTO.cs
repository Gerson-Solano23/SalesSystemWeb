using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class EmailDTO
    {
        [DataType(DataType.EmailAddress)]
        public string? sender_User { get; set; }
        public string? password_Sender { get; set; }
        [DataType(DataType.EmailAddress)]
        public string? recipient_User { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public byte[]? Attached { get; set; }

    }
}
