using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Interfaces.Repositories;

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
            return await _dbSet.AsNoTracking().ToListAsync();
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

    }
}
