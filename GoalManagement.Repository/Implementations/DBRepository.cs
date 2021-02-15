using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GoalManagement.Domain.Interfaces;
using GoalManagement.Repository.Context;
using System.Threading.Tasks;

namespace GoalManagement.Repository.Implementations
{
    public class DBRepository<T> : IRepository<T> where T : class
    {
        private readonly GoalManagementContext context;
        private readonly DbSet<T> dbSet;

        public DBRepository(GoalManagementContext _context)
        {
            context = _context;
            dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IQueryable<T> FindAll()
        {
            return dbSet;
        }

        public T FindById(object id)
        {
            return dbSet.Find(id);
        }

        public async Task<T> FindByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public void Remove(object id)
        {
            var entity = FindById(id);

            if (entity == null) return;

            if (context.Entry(entity).State == EntityState.Deleted) return;

            dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            var dbEntityEntry = context.Entry(entity);

            if (dbEntityEntry.State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            dbEntityEntry.State = EntityState.Modified;
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        #region IDisposable implementations
        private bool isDisposed = false;

        protected virtual void Dispose(bool isDispose)
        {
            if (!isDisposed)
            {
                if (isDispose)
                {
                    context?.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
