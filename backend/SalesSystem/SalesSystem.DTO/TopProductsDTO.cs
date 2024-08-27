using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class TopProductsDTO
    {
        [Key]
        public int topNumber { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int salequantity { get; set; }     
        public decimal totalSale { get; set; }
    }
}
