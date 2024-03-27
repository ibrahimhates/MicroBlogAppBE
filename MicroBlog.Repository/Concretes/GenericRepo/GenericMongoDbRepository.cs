using System.Linq.Expressions;
using MicroBlog.Core.Abstractions.Repositories.GenericRepo;
using MicroBlog.Core.Entities.Base;
using MicroBlog.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroBlog.Repository.Concretes.GenericRepo;

public class GenericMongoDbRepository<T,TKey> : IGenericMongoDbRepository<T,TKey>
    where TKey : struct
    where T : class, IEntity<TKey>
{
    private readonly IMongoCollection<T> _collection;

    public GenericMongoDbRepository(IOptions<MongoDbOptions> options,string collectionName)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _collection = mongoClient
            .GetDatabase(options.Value.DatabaseName)
            .GetCollection<T>(collectionName);

    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var documents = await _collection.Find(_ => true)
            .ToListAsync();

        return documents;
    }

    public async Task<T> GetByCondition(Expression<Func<T, bool>> expression)
    {
        var collection = await _collection
            .Find(expression)
            .FirstOrDefaultAsync();

        return collection;
    }

    public async Task<T> GetByIdAsync(TKey id)
    {
        var document = await _collection
            .Find(x => x.Id.ToString() == id.ToString())
            .FirstOrDefaultAsync();

        return document;
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        var result = await _collection
            .Find(expression)
            .AnyAsync();
        
        return result;
    }

    public async Task CreateAsync(T document)
    {
        await _collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(T document)
    {
        await _collection
            .ReplaceOneAsync(x => x.Id.ToString() == document.Id.ToString(), document);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> expression)
    {
        await _collection.FindOneAndDeleteAsync(expression);
    }
}