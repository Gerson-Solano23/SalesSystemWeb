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
        private readonly IGenericRepository<Usuario> _userRepository;
        private readonly IGenericRepository<MenuRol> _menuRolRepository;
        private readonly IGenericRepository<Menu> _menuRepository;
        private readonly IMapper _mapper;

        public MenuService(IGenericRepository<Usuario> userRepository, IGenericRepository<MenuRol> menuRolRepository, IGenericRepository<Menu> menuRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _menuRolRepository = menuRolRepository;
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public async Task<List<MenuDTO>> List(int idUser)
        {
            IQueryable<Usuario> userTB = await _userRepository.Consult(x => x.IdUser == idUser);
            IQueryable<MenuRol> menuRolTB = await _menuRolRepository.Consult();
            IQueryable<Menu> manuTB = await _menuRepository.Consult();

            try
            {
                IQueryable<Menu> resultTB = (from u in userTB
                                             join mr in menuRolTB on u.IdRol equals mr.IdRol
                                             join m in manuTB on mr.IdMenu equals m.IdMenu 
                                             select m).AsQueryable();

                var menuList = resultTB.ToList();

                return _mapper.Map<List<MenuDTO>>(menuList);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
