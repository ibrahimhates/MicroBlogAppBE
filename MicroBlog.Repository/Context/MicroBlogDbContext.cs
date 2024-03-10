using System.Reflection;
using MicroBlog.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Repository.Context;

public class MicroBlogDbContext : DbContext
{
    public MicroBlogDbContext(DbContextOptions options) : base(options)
    { }
    private DbSet<User> Users { get; set; }
    private DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}