using System.Linq.Expressions;
using MicroBlog.Core.Entities.Base;
using MicroBlog.Core.Repositories.GenericRepo;
using MicroBlog.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Repository.Concretes.GenericRepo;

public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
{
    private readonly MicroBlogDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(MicroBlogDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> GetAll(bool trackChanges) =>
        trackChanges ? _dbSet.AsTracking() : _dbSet.AsNoTracking();

    public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges) =>
        trackChanges ? _dbSet.Where(expression).AsTracking() : _dbSet.Where(expression).AsNoTracking();

    public async Task<T> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression) => await _dbSet.AnyAsync(expression);

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(entity, cancellationToken);

    public async Task CreateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
        await _dbSet.AddRangeAsync(entities, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task<int> GetCountAsync() => await _dbSet.CountAsync();
}