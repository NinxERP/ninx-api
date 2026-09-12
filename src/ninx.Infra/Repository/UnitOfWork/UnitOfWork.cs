using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ninx.Data.Context;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

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
            if (_transaction == null)
                throw new InvalidOperationException("CommitAsync foi chamado sem uma transação aberta (BeginTransactionAsync).");

            try
            {
                await _transaction.CommitAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ninx.Domain.Exceptions.ConcurrencyException("Erro de concorrência ao finalizar a transação.");
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("RollbackAsync foi chamado sem uma transação aberta (BeginTransactionAsync).");

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("O estoque foi alterado por outro usuário. Tente novamente.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new BadRequestException("Já existe um registro com esse valor. Verifique os dados informados.");
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operacao)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await operacao();
                return true;
            });
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operacao)
        {
            await BeginTransactionAsync();
            try
            {
                var resultado = await operacao();
                await SaveChangesAsync();
                await CommitAsync();
                return resultado;
            }
            catch
            {
                if (_transaction != null)
                    await RollbackAsync();
                throw;
            }
        }
    }
}
