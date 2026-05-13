namespace ninx.Domain.Interfaces
{
    public interface IRepositoryBase<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<(IEnumerable<TEntity> Data, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<TEntity?> GetByIdAsync(params object[] parameters);
        void Delete(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<TEntity> AddAsync(TEntity entity);
        Task AddBatchAsync(IEnumerable<TEntity> entities);
        Task UpdateBatchAsync(IEnumerable<TEntity> entities);
    }
}
