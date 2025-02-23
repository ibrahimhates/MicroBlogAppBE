using System.Linq.Expressions;
using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Repositories.GenericRepo;

public interface IGenericRepository<T,TKey> 
    where TKey : struct
    where T : class, IEntity<TKey>
{
    /// <summary>
    /// This method returns all available data 
    /// </summary>
    /// <remarks>
    /// trackChanges is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="trackChanges">A value to track changes</param>
    /// <returns>
    /// An <see cref="IQueryable{out T}"/> that represents the input sequence.
    /// </returns>
    IQueryable<T> GetAll(bool trackChanges);
    
    /// <summary>
    /// This method returns all available data as per condition
    /// </summary>
    /// <remarks>
    /// trackChanges is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="trackChanges">A value to track changes</param>
    /// <param name="expression">A value to lambda expression</param>
    /// <returns>
    /// An <see cref="IQueryable{out T}"/> that represents the input sequence.
    /// </returns>
    IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    
    /// <summary>
    /// This method returns a data by id
    /// </summary>
    /// <remarks>
    /// id is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="id">A value to Guid</param>
    /// <returns>
    /// An <see cref="Task{T}"/> that represents the input sequence.
    /// </returns>
    Task<T> GetByIdAsync(TKey id);

    /// <summary>
    /// This method returns whether there is current data based on the condition
    /// </summary>
    /// <remarks>
    /// expression is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="expression">A value to lambda expression</param>
    /// <returns>
    /// An <see cref="Task{T}"/> that represents the input sequence. TResult is boolean
    /// </returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    
    /// <summary>
    /// This method adds a record asynchronously
    /// </summary>
    /// <remarks>
    /// entity is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="entity">A value to record</param>
    /// <returns>
    /// An <see cref="Task"/> that represents the input sequence
    /// </returns>
    Task CreateAsync(T entity,CancellationToken cancellationToken = default);
    
    /// <summary>
    /// This method adds multiple records asynchronously
    /// </summary>
    /// <remarks>
    /// entities is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="entities">A value to multiple record</param>
    /// <returns>
    /// An <see cref="Task"/> that represents the input sequence
    /// </returns>
    Task CreateAsync(IEnumerable<T> entities,CancellationToken cancellationToken = default);
    
    /// <summary>
    /// This method updates a record simultaneously
    /// </summary>
    /// <remarks>
    /// entity is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="entity">A value to record</param>
    /// <returns>
    /// Have no value.
    /// </returns>
    void Update(T entity);

    /// <summary>
    /// This method delete a record simultaneously
    /// </summary>
    /// <remarks>
    /// entity is required
    /// </remarks>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="entity">A value to record</param>
    /// <returns>
    /// Have no value.
    /// </returns>
    void Delete(T entity);

    /// <summary>
    /// This method returns the count of the dataset
    /// </summary>
    /// <returns>
    /// An <see cref="Task{T}"/> that represents the Integer
    /// </returns>
    Task<int> GetCountAsync();
}