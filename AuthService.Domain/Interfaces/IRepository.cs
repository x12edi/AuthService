using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IRepository<T> where T : class 
    { 
        Task<T> GetByIdAsync(string id); 
        Task<IEnumerable<T>> GetAllAsync(); 
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate); 
        Task AddAsync(T entity); 
        void Update(T entity); 
        void Delete(T entity); 
    }
}
