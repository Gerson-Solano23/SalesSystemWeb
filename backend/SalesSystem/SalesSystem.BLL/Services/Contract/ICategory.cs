using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface ICategory
    {
        Task<List<CategoryDTO>> List();
        //Task<CategoryDTO> Create(CategoryDTO entity);
        //Task<bool> Update(CategoryDTO entity);
        //Task<bool> Delete(int id);
        //Task<CategoryDTO> Get(int id);
    }
}
