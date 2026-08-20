using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    /// <summary>
    /// Centraliza a checagem de hierarquia por peso entre Cargos de um mesmo vínculo (UsuarioComercio),
    /// distinta do administrador de plataforma (Usuario.Admin, ver AutorizacaoGlobalService), que sempre
    /// tem bypass total sobre essas regras.
    /// </summary>
    public class AutorizacaoCargoService : IAutorizacaoCargoService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public AutorizacaoCargoService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<bool> EhAdminGlobalAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            return usuario is not null && usuario.Admin;
        }

        public void GarantirGerencia(bool chamadorEhAdminGlobal, int pesoChamador, int pesoAlvo)
        {
            if (chamadorEhAdminGlobal) return;

            if (pesoChamador <= pesoAlvo)
                throw new ForbiddenException("Você não tem permissão para gerenciar um vínculo com esse cargo.");
        }

        public void GarantirPesoMinimo(bool chamadorEhAdminGlobal, int pesoChamador, int pesoMinimo, string mensagemErro)
        {
            if (chamadorEhAdminGlobal) return;

            if (pesoChamador < pesoMinimo)
                throw new ForbiddenException(mensagemErro);
        }
    }
}
