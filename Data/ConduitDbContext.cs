using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class ConduitDbContext : DbContext
    {
        public ConduitDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder model)
        {
            // article
            model.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(a => a.Articles);

            // comment
            model.Entity<Comment>().HasKey(c => new
            {
                c.ArticleId,
                c.CommentId
            });
            model.Entity<Comment>()
                .Property(c => c.CommentId)
                .UseSqlServerIdentityColumn();
            model.Entity<Comment>()
                .HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            model.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // favorite
            model.Entity<Favorite>().HasKey(o => new
            {
                o.ArticleId,
                o.UserId
            });
            model.Entity<Favorite>().HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            model.Entity<Favorite>().HasOne(f => f.Article)
                .WithMany(u => u.Favorites)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // follow
            model.Entity<Follow>().HasKey(o => new
            {
                o.FollowingUserId,
                o.FollowedUserId
            });
            model.Entity<Follow>().HasOne(f => f.FollowingUser)
                .WithMany(u => u.Followings)
                .HasForeignKey(z => z.FollowingUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            model.Entity<Follow>().HasOne(f => f.FollowedUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(z => z.FollowedUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // tag
            model.Entity<Tag>().HasKey(o => new
            {
                o.ArticleId,
                o.TagName
            });
            model.Entity<Tag>().HasIndex(t => t.TagName);
            model.Entity<Tag>().HasOne(t => t.Article)
                .WithMany(a => a.Tags)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            model.Entity<User>().HasMany(u => u.Comments);
        }

        public void DeleteArticle(int articleId)
        {
            // deleting via query will be more efficient and avoid cascading issues.
            string query = @"DELETE FROM Favorites WHERE ArticleId = @p0
                    DELETE FROM Comments WHERE ArticleId = @p0
                    DELETE FROM Tags WHERE ArticleId = @p0
                    DELETE FROM Articles WHERE Id = @p0";
            Database.ExecuteSqlCommand(query, articleId);
        }
    }
}
