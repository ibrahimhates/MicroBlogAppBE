using System.Linq.Expressions;
using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Abstractions.Repositories.GenericRepo;

public interface IGenericMongoDbRepository<T,TKey> 
    where TKey : struct
    where T : class, IEntity<TKey>
    
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByCondition(Expression<Func<T, bool>> expression);
    Task<T> GetByIdAsync(TKey id);
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    Task CreateAsync(T document);
    Task UpdateAsync(T document);
    Task DeleteAsync(Expression<Func<T, bool>> expression);
    Task DeleteByIdAsync(TKey id);
}