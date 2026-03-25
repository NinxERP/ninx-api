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
            await _transaction!.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
