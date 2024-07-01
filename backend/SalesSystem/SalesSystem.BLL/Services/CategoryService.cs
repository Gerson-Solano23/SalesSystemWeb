using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class CategoryService : ICategory
    {

        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IGenericRepository<Category> repositoryCategory, IMapper mapper)
        {
            _mapper = mapper;
            _categoryRepository = repositoryCategory;
        }
        public async Task<List<CategoryDTO>> List()
        {
            try
            {
                var queryUsuario = await _categoryRepository.GetAllAsync();
                //var categoryList = queryUsuario.Include(rol => rol.IdCategory).ToList();

                return _mapper.Map<List<CategoryDTO>>(queryUsuario);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
