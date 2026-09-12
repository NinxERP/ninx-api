namespace ninx.Application.Services
{
    public interface IAutorizacaoCargoService
    {
        Task<bool> EhAdminGlobalAsync(int usuarioId);
        void GarantirPermissao(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, IEnumerable<string> permissoesChamador, string permissaoRequerida, string mensagemErro);
        void GarantirNaoProprietario(bool chamadorEhAdminGlobal, bool ehProprietario, string mensagemErro);
        void GarantirProprietario(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, string mensagemErro);
        void GarantirSemEscalonamento(bool chamadorEhAdminGlobal, bool chamadorEhProprietario, IEnumerable<string> permissoesChamador, IEnumerable<string> permissoesRequisitadas, string mensagemErro);
    }
}
