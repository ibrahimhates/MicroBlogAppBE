using MicroBlog.Repository.Context;

namespace MicroBlog.Repository.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly MicroBlogDbContext _context;

    public UnitOfWork(MicroBlogDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}