using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DAL.Repositories.Contract
{
    public interface ISalesRepository:IGenericRepository<Sale>
    {
        Task<Sale>  addRegistry(Sale sale);
        //Task addRegistry(SalesSystem.DTO.SaleDTO saleDTO);
    }
}
