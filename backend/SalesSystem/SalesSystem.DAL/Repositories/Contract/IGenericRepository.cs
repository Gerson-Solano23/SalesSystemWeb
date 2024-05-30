using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
namespace SalesSystem.DAL.Repositories.Contract
{
    public interface IGenericRepository<TModel> where TModel : class
    {
        Task<List<TModel>> GetAllAsync();
        Task<TModel> GetAsync(Expression<Func<TModel, bool>> filter);
        Task<TModel> GetByIdAsync(int id);
        Task<TModel> AddAsync(TModel entity);
        Task<bool> UpdateAsync(TModel entity);
        Task<bool> DeleteAsync(TModel entity);      

        Task<IQueryable<TModel>> Consult(Expression<Func<TModel, bool>> filter = null);
    }   
   
}