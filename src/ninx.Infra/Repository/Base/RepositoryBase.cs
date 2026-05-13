using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        private readonly NinxDB _context;
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryBase(NinxDB context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking()
                .ToListAsync();
        }

        public async Task<(IEnumerable<TEntity> Data, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.AsNoTracking();

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<TEntity?> GetByIdAsync(params object[] parameters)
        {
            return await _dbSet.FindAsync(parameters);
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public async Task AddBatchAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task UpdateBatchAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
        }
    }
}
