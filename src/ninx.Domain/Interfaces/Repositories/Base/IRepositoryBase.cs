namespace ninx.Domain.Interfaces.Repositories.Base
{
        public interface IRepositoryBase<TEntity> where TEntity : class
        {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(params object[] parameters);
        void Delete(TEntity entity);
        Task <TEntity> Update(TEntity entity);
        Task <TEntity> AddAsync(TEntity entity);
        Task SaveChangesAsync();

    }
}
