namespace ninx.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> operacao);
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operacao);
    }
}
