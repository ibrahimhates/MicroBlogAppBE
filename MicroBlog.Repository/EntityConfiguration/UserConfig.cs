using MicroBlog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Repository.EntityConfiguration;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.Ignore(x => x.FullName);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.UserToken)
            .WithOne(x => x.User)
            .HasForeignKey<UserToken>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}