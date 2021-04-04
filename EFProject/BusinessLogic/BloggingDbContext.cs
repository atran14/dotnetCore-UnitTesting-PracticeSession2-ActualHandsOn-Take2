using System;
using EFProject.Models;
using Microsoft.EntityFrameworkCore;

namespace EFProject.BusinessLogic
{
    public class BloggingDbContext : DbContext
    {

        public DbSet<Blog> Blogs {get; set;}
        public DbSet<Post> Posts {get; set;}

        public BloggingDbContext()
        { }

        public BloggingDbContext(DbContextOptions<BloggingDbContext> contextOptions)
        : base(contextOptions)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlServer("Server=localhost;Database=testDB_UnitTestingVer;Trusted_Connection=True;");
            }            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Constraints for Blog
            builder.Entity<Blog>()
                .Property(blog => blog.Url)
                .IsRequired();

            //Constraints for Post
            builder.Entity<Post>()
                .Property(post => post.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Post>()
                .HasIndex(p => p.Title)
                .IsUnique();

            builder.Entity<Post>()
                .HasOne<Blog>(p => p.Blog)
                .WithMany(b => b.Posts)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
