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
    public class UsuarioService : IUsuarioService
    {

        private readonly IGenericRepository<Usuario> _usuarioRepository;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> repositoryUsuario, IMapper mapper)
        {
            _mapper = mapper;
            _usuarioRepository = repositoryUsuario;
        }

        public async Task<UsuarioDTO> Create(UsuarioDTO entity)
        {
            try
            {
                var user = await _usuarioRepository.AddAsync(_mapper.Map<Usuario>(entity));

                if (user.IdUser == 0)
                {
                    throw new TaskCanceledException("Unable to create");
                }

                var query = await _usuarioRepository.Consult(x => x.Email == entity.Email);

                user = query.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(user);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {            
                var userFound = await _usuarioRepository.GetByIdAsync(id);

                if (userFound == null)
                {
                    throw new TaskCanceledException("User not exist");
                }

                bool result = await _usuarioRepository.DeleteAsync(userFound);

                if (result)
                {
                    throw new TaskCanceledException("Failed to delete");
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UsuarioDTO> Get(int id)
        {
            try
            {
                var userFound = await _usuarioRepository.GetByIdAsync(id);

                if (userFound == null)
                {
                    throw new TaskCanceledException("User not exist");
                }

                return _mapper.Map<UsuarioDTO>(userFound);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<UsuarioDTO>> List()
        {
            try
            {
                var queryUsuario = await _usuarioRepository.Consult();
                var userList = queryUsuario.Include(rol => rol.IdRolNavigation).ToList();

                return _mapper.Map<List<UsuarioDTO>>(userList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Update(UsuarioDTO entity)
        {
            try
            {
                var userEntity = _mapper.Map<Usuario>(entity);

                var userFound = _usuarioRepository.GetByIdAsync(entity.IdUser);

                if (userFound.Result == null)
                {
                    throw new TaskCanceledException("User not exist");
                }

                bool result = await _usuarioRepository.UpdateAsync(userEntity);

                if (result)
                {
                    throw new TaskCanceledException("Failed to update");
                }

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<SessionDTO> ValidateCredentials(string email, string password)
        {
            SessionDTO session = new SessionDTO();
            try
            {
                var query = await _usuarioRepository.Consult(x => x.Email == email && x.Password == password);

                if (query.FirstOrDefault() == null)
                {
                    throw new TaskCanceledException("User not exist");
                }
                else
                {
                    Usuario usuario = query.Include(rol => rol.IdRolNavigation).First();
                    return _mapper.Map<SessionDTO>(usuario);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
