using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;

namespace SalesSystem.BLL.Services
{
    public class RolService
    {
        private readonly IGenericRepository<Rol> _rolRepository;
        
        private readonly IMapper _mapper;

        public RolService(IGenericRepository<Rol> repositoryRol, IMapper mapper)
        {
            _mapper = mapper;
            _rolRepository = repositoryRol;
        }

        public async Task<List<RolDTO>> List()
        {
            try
            {
                var rolList = await _rolRepository.Consult();

                return _mapper.Map<List<RolDTO>>(rolList.ToList());

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
