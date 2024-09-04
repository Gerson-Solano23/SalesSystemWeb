using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class SaleToPredictDTO
    {
        public string PaymentType { get; set; }
        public float Total { get; set; }
        public float DateRegistryAsNumber { get; set; }
    }
}
