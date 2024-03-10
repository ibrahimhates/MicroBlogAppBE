using MicroBlog.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroBlogAppBE.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSqlContext(this IServiceCollection services,
        IConfiguration configuration) => services
        .AddDbContext<MicroBlogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("sqlConnection"))
        );
}