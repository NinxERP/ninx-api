using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class PessoaAutorizadaRepository : RepositoryBase<PessoaAutorizada>, IPessoaAutorizadaRepository
    {
        private readonly NinxDB _context;
        public PessoaAutorizadaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PessoaAutorizada>> GetPorClienteAsync(int clienteId)
        {
            return await _context.PessoasAutorizadas
                .Where(p => p.ClienteID == clienteId)
                .OrderBy(p => p.CriadoEm)
                .ToListAsync();
        }

        public async Task<PessoaAutorizada?> GetDoClienteAsync(int pessoaAutorizadaId, int clienteId)
        {
            return await _context.PessoasAutorizadas
                .FirstOrDefaultAsync(p => p.PessoaAutorizadaID == pessoaAutorizadaId && p.ClienteID == clienteId);
        }
    }
}
