namespace ninx.Application.Services
{
    public interface IAutorizacaoCargoService
    {
        Task<bool> EhAdminGlobalAsync(int usuarioId);
        void GarantirGerencia(bool chamadorEhAdminGlobal, int pesoChamador, int pesoAlvo);
        void GarantirPesoMinimo(bool chamadorEhAdminGlobal, int pesoChamador, int pesoMinimo, string mensagemErro);
    }
}
