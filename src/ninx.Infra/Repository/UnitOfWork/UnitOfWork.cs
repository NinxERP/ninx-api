using Microsoft.EntityFrameworkCore.Storage;
using ninx.Data.Context;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NinxDB _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(NinxDB context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ninx.Domain.Exceptions.ConcurrencyException("Erro de concorrência ao finalizar a transação.");
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ninx.Domain.Exceptions.ConcurrencyException("O estoque foi alterado por outro usuário. Tente novamente.");
            }
        }
    }
}
