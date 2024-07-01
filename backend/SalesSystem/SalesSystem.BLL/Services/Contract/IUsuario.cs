using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SalesSystem.DTO;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IUsuario
    {
        Task<List<UsuarioDTO>> List();
        Task<SessionDTO> ValidateCredentials(string email, string password);
        Task<UsuarioDTO> Create(UsuarioDTO entity);
        Task<bool> Update(UsuarioDTO entity);
        Task<bool> Delete(int id);

        Task<UsuarioDTO> Get(int id);

    }
}
