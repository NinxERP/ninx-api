using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    /// <summary>
    /// Centraliza a checagem de administrador de plataforma (Usuario.Admin), distinta da
    /// hierarquia por peso de Cargo (ver AutorizacaoCargoService) usada nas ações dentro de um comércio.
    /// </summary>
    public class AutorizacaoGlobalService : IAutorizacaoGlobalService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public AutorizacaoGlobalService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario> GarantirAdministradorGlobalAsync(int usuarioIdLogado)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioIdLogado);
            if (usuario is null || !usuario.Admin)
                throw new UnauthorizedException("Você não possui permissão para utilizar esse endpoint");

            return usuario;
        }
    }
}
