using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IProduct
    {
        Task<List<ProductDTO>> List();
        Task<ProductDTO> Create(ProductDTO entity);
        Task<bool> Update(ProductDTO entity);
        Task<bool> Delete(int id);
        Task<ProductDTO> Get(int id);
    }
}
