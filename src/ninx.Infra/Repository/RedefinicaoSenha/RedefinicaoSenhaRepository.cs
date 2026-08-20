using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class RedefinicaoSenhaRepository : RepositoryBase<RedefinicaoSenha>, IRedefinicaoSenhaRepository
    {
        private readonly NinxDB _context;

        public RedefinicaoSenhaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<RedefinicaoSenha?> GetUltimoAtivoAsync(int usuarioId)
        {
            return await _context.RedefinicoesSenha
                .Where(x => x.UsuarioID == usuarioId && !x.Utilizado && x.ExpiraEm > DateTime.UtcNow)
                .OrderByDescending(x => x.CriadoEm)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidarAtivosAsync(int usuarioId)
        {
            var ativos = await _context.RedefinicoesSenha
                .Where(x => x.UsuarioID == usuarioId && !x.Utilizado && x.ExpiraEm > DateTime.UtcNow)
                .ToListAsync();

            foreach (var ativo in ativos)
                ativo.Utilizado = true;

            _context.RedefinicoesSenha.UpdateRange(ativos);
        }
    }
}
