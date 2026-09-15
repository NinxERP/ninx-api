using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class TermoAberturaContaRepository : RepositoryBase<TermoAberturaConta>, ITermoAberturaContaRepository
    {
        private readonly NinxDB _context;
        public TermoAberturaContaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<TermoAberturaConta?> GetAtivoAsync(int clienteId)
        {
            return await _context.TermosAberturaConta
                .FirstOrDefaultAsync(t => t.ClienteID == clienteId && t.Status == StatusTermoAbertura.Ativo);
        }

        public async Task<List<TermoAberturaConta>> GetAguardandoAsync(int clienteId)
        {
            return await _context.TermosAberturaConta
                .Where(t => t.ClienteID == clienteId && t.Status == StatusTermoAbertura.Aguardando)
                .ToListAsync();
        }

        public async Task<List<TermoAberturaConta>> GetPorClienteAsync(int clienteId)
        {
            return await _context.TermosAberturaConta
                .Where(t => t.ClienteID == clienteId)
                .OrderByDescending(t => t.Versao)
                .ToListAsync();
        }
    }
}
