using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class PagamentoVendaRepository : RepositoryBase<PagamentoVenda>, IPagamentoVendaRepository
    {
        private readonly NinxDB _context;
        public PagamentoVendaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
    }
}
