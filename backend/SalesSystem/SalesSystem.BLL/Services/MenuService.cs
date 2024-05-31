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
    public class MenuService : IMenu
    {
        private readonly IGenericRepository<Menu> _menuRepository;
        private readonly IMapper _mapper;

        public MenuService(IGenericRepository<Menu> repositoryMenu, IMapper mapper)
        {
            _mapper = mapper;
            _menuRepository = repositoryMenu;
        }

        public async Task<List<MenuDTO>> List()
        {
            try
            {
                var query = await _menuRepository.Consult();

                var menuList = query.Include(rol => rol.IdMenu).ToList();

                return _mapper.Map<List<MenuDTO>>(menuList);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
