using MicroBlog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Repository.EntityConfiguration;

public class FollowerConfig : IEntityTypeConfiguration<Follower>
{
    public void Configure(EntityTypeBuilder<Follower> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Followers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.FollowerUser)
            .WithMany()
            .HasForeignKey(x => x.FollowerUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}