using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IRedefinicaoSenhaRepository : IRepositoryBase<RedefinicaoSenha>
    {
        Task<RedefinicaoSenha?> GetUltimoAtivoAsync(int usuarioId);
        Task InvalidarAtivosAsync(int usuarioId);
    }
}
