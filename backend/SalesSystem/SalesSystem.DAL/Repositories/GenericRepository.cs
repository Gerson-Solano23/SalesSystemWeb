using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;

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
                await _context.Set<TModel>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error creating entity: {ex.Message}", ex);
            }
        }

        public async Task<List<TModel>> GetAllAsync()
        {
            try
            {
                return await _context.Set<TModel>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error getting all entities: {ex.Message}", ex);
            }
        }

        public async Task<TModel> GetAsync(Expression<Func<TModel, bool>> filter)
        {
            try
            {
                return await _context.Set<TModel>().AsNoTracking().FirstOrDefaultAsync(filter);
            }
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error getting entity: {ex.Message}", ex);
            }
        }

        public async Task<TModel> GetByIdAsync(int id)
        {
            try
            {
                var keyName = _context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties.Select(x => x.Name).Single();
                return await _context.Set<TModel>().AsNoTracking().SingleOrDefaultAsync(e => EF.Property<int>(e, keyName) == id);
            }
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error getting entity by id: {ex.Message}", ex);
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
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error updating entity: {ex.Message}", ex);
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
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error deleting entity: {ex.Message}", ex);
            }
        }

        public async Task<IQueryable<TModel>> Consult(Expression<Func<TModel, bool>> filter = null)
        {
            try
            {
                return filter == null ? _context.Set<TModel>().AsNoTracking() : _context.Set<TModel>().AsNoTracking().Where(filter);
            }
            catch (Exception ex)
            {
                // Agregar logging aquí
                throw new Exception($"Error consulting entities: {ex.Message}", ex);
            }
        }
    }
}
