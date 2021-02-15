using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoalManagement.Domain.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class
    {
        void Add(T entity);
        T FindById(object id);
        Task<T> FindByIdAsync(object id);
        void Remove(object id);
        void Update(T entity);
        IQueryable<T> FindAll();
        Task SaveAsync();
    }
}
