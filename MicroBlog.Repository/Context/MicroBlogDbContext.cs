using System.Reflection;
using MicroBlog.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Repository.Context;

public class MicroBlogDbContext : DbContext
{
    public MicroBlogDbContext(DbContextOptions<MicroBlogDbContext> options) : base(options)
    { }
    
    DbSet<User> Users { get; set; }
    DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}