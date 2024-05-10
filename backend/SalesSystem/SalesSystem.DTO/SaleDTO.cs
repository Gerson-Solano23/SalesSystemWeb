using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class SaleDTO
    {
        public int IdSale { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? PaymentType { get; set; }
        public string? Total_Text { get; set; }
        public string? DateRegistry { get; set; }

        public virtual ICollection<SaleDetailDTO> SaleDetails { get; set; }
    }
}
