using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Repositories.Interfaces
{
    public interface IRepository<TType> where TType : class
    {
        TType GetById(Guid id);

        Task<TType> GetByIdAsync(Guid id);

        TType FirstOrDefault(Func<TType, bool> predicate);

        Task<TType> FirstOrDefaultAsync(Expression<Func<TType, bool>> predicate);

        IEnumerable<TType> GetAll();

        Task<IEnumerable<TType>> GetAllAsync();

        IQueryable<TType> GetAllAttached();

        void Add(TType item);

        Task AddAsync(TType item);

        void AddRange(TType[] items);

        Task AddRangeAsync(TType[] items);

        bool Delete(TType entity);

        Task<bool> DeleteAsync(TType entity);

        bool Update(TType item);

        Task<bool> UpdateAsync(TType item);
    }
}
