using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    /// <summary>
    /// Centraliza a checagem de permissões granulares do Cargo de um vínculo (UsuarioComercio),
    /// distinta do administrador de plataforma (Usuario.Admin, ver AutorizacaoGlobalService), que sempre
    /// tem bypass total sobre essas regras, e do cargo "proprietário" (Dono), que também tem bypass
    /// sobre as permissões comuns dentro do próprio comércio.
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

        public void GarantirPermissao(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, IEnumerable<string> permissoesChamador, string permissaoRequerida, string mensagemErro)
        {
            if (chamadorEhAdminGlobal || chamadorEhProprietario) return;

            if (!permissoesChamador.Contains(permissaoRequerida))
                throw new ForbiddenException(mensagemErro);
        }

        public void GarantirNaoProprietario(bool chamadorEhAdminGlobal, bool ehProprietario, string mensagemErro)
        {
            if (chamadorEhAdminGlobal) return;

            if (ehProprietario)
                throw new ForbiddenException(mensagemErro);
        }

        public void GarantirProprietario(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, string mensagemErro)
        {
            if (chamadorEhAdminGlobal || chamadorEhProprietario) return;

            throw new ForbiddenException(mensagemErro);
        }

        public void GarantirSemEscalonamento(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, IEnumerable<string> permissoesChamador, IEnumerable<string> permissoesRequisitadas, string mensagemErro)
        {
            if (chamadorEhAdminGlobal || chamadorEhProprietario) return;

            if (permissoesRequisitadas.Except(permissoesChamador).Any())
                throw new ForbiddenException(mensagemErro);
        }
    }
}
