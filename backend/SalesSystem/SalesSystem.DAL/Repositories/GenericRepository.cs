using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DAL.DbSalesContext;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
namespace SalesSystem.DAL.Repositories
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {
        private readonly DBSalesContext _context;
        public GenericRepository(DBSalesContext dBSalesContext)
        {
            _context = dBSalesContext;
        }
        public async Task<TModel> CreateAsync(TModel entity)
        {
            try
            {
                _context.Set<TModel>().Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception)
            {

                throw;
            }
        }   

        public async Task<List<TModel>> GetAllAsync()
        {
            try
            {
                List<TModel> modelList = await _context.Set<TModel>().ToListAsync();
                return modelList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TModel> GetAsync(Expression<Func<TModel, bool>> filter)
        {
            try
            {
                TModel model = await _context.Set<TModel>().FindAsync(filter);
                return model;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TModel> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<TModel>().FindAsync(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdateAsync(TModel entity)
        {
            try
            {
                _context.Set<TModel>().Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> DeleteAsync(TModel entity)
        {
            try
            {
                _context.Set<TModel>().Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
        public async Task<IQueryable<TModel>> Consult(Expression<Func<TModel, bool>> filter = null)
        {
            try
            {
                IQueryable<TModel> queryEntity = filter == null? _context.Set<TModel>() : _context.Set<TModel>().Where(filter);
                return queryEntity;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
