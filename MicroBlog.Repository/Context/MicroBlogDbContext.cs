using System.Reflection;
using MicroBlog.Core.Entities;
using MicroBlog.Core.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Repository.Context;

public class MicroBlogDbContext : DbContext
{
    public MicroBlogDbContext(DbContextOptions<MicroBlogDbContext> options) : base(options)
    { }
    
    DbSet<User> Users { get; set; }
    DbSet<UserToken> UserTokens { get; set; }
    DbSet<Follower> Followers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added 
                                                   | x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((BaseEntity)entity.Entity).CreatedDate = now;
            }
            if (entity.State == EntityState.Modified)
                ((BaseEntity)entity.Entity).UpdatedDate = now;
        }
    }
}