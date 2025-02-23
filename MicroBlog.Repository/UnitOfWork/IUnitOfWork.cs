namespace MicroBlog.Repository.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}