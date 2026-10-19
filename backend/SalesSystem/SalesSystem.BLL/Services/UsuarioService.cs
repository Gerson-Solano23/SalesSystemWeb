using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using SalesSystem.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class UsuarioService : IUsuario
    {
        private const int MinPasswordLength = 8;

        private readonly IGenericRepository<Usuario> _usuarioRepository;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> repositoryUsuario, IMapper mapper)
        {
            _mapper = mapper;
            _usuarioRepository = repositoryUsuario;
        }

        public async Task<UsuarioDTO> Create(UsuarioDTO entity)
        {
            ValidatePasswordStrength(entity.Password);

            if (!await ValidateEmail(entity.Email))
            {
                throw new BusinessException("Email already exists");
            }

            var newUser = _mapper.Map<Usuario>(entity);
            newUser.Password = PasswordHasher.Hash(entity.Password!);
            newUser.DateRegistry = DateTime.Now;

            var user = await _usuarioRepository.CreateAsync(newUser);

            if (user.IdUser == 0)
            {
                throw new BusinessException("Unable to create");
            }

            var query = await _usuarioRepository.Consult(x => x.IdUser == user.IdUser);
            user = await query.Include(rol => rol.IdRolNavigation).FirstAsync();

            return _mapper.Map<UsuarioDTO>(user);
        }

        public async Task<bool> Delete(int id)
        {
            var userFound = await _usuarioRepository.GetByIdAsync(id);

            if (userFound == null)
            {
                throw BusinessException.NotFound("User not exist");
            }

            return await _usuarioRepository.DeleteAsync(userFound);
        }

        public async Task<UsuarioDTO> Get(int id)
        {
            var query = await _usuarioRepository.Consult(x => x.IdUser == id);
            var userFound = await query.Include(rol => rol.IdRolNavigation).FirstOrDefaultAsync();

            if (userFound == null)
            {
                throw BusinessException.NotFound("User not exist");
            }

            return _mapper.Map<UsuarioDTO>(userFound);
        }

        public async Task<List<UsuarioDTO>> List()
        {
            var queryUsuario = await _usuarioRepository.Consult();
            var userList = await queryUsuario.Include(rol => rol.IdRolNavigation).ToListAsync();

            return _mapper.Map<List<UsuarioDTO>>(userList);
        }

        public async Task<bool> Update(UsuarioDTO entity)
        {
            var userFound = await _usuarioRepository.GetByIdAsync(entity.IdUser);

            if (userFound == null)
            {
                throw BusinessException.NotFound("User not exist");
            }

            if (!string.Equals(userFound.Email, entity.Email, StringComparison.OrdinalIgnoreCase)
                && !await ValidateEmail(entity.Email))
            {
                throw new BusinessException("Email already exists");
            }

            var changes = _mapper.Map<Usuario>(entity);
            userFound.FullName = changes.FullName;
            userFound.Email = changes.Email;
            userFound.IdRol = changes.IdRol;
            userFound.Status = changes.Status;

            // An empty password on update means "keep the current one".
            if (!string.IsNullOrWhiteSpace(entity.Password))
            {
                ValidatePasswordStrength(entity.Password);
                userFound.Password = PasswordHasher.Hash(entity.Password);
            }

            return await _usuarioRepository.UpdateAsync(userFound);
        }

        /// <summary>
        /// Returns the session for valid, active credentials and null otherwise.
        /// Passwords still stored in plain text are re-hashed on the first successful login.
        /// </summary>
        public async Task<SessionDTO?> ValidateCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var query = await _usuarioRepository.Consult(x => x.Email == email);
            Usuario? usuario = await query.Include(rol => rol.IdRolNavigation).FirstOrDefaultAsync();

            if (usuario == null || !PasswordHasher.Verify(password, usuario.Password, out bool needsRehash))
            {
                return null;
            }

            if (usuario.Status != true)
            {
                throw new BusinessException("User is inactive", 403);
            }

            if (needsRehash)
            {
                var navigation = usuario.IdRolNavigation;
                usuario.IdRolNavigation = null;
                usuario.Password = PasswordHasher.Hash(password);
                await _usuarioRepository.UpdateAsync(usuario);
                usuario.IdRolNavigation = navigation;
            }

            return _mapper.Map<SessionDTO>(usuario);
        }

        public async Task<bool> ValidateEmail(string? email)
        {
            var query = await _usuarioRepository.Consult(x => x.Email == email);
            return !await query.AnyAsync();
        }

        private static void ValidatePasswordStrength(string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            {
                throw new BusinessException($"Password must be at least {MinPasswordLength} characters long");
            }
        }
    }
}
