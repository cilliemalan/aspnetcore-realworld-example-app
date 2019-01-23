using ConduitApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class CommentsController : ApiController
    {
        private readonly Data.ConduitDbContext _dbContext;

        public CommentsController(Data.ConduitDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpPost]
        [Route("articles/{slug}/comments")]
        public Envelope<Comment> NewComment(string slug, [FromBody]Envelope<NewComment> comment)
        {
            EnsureModelValid();

            var dbArticleId = _dbContext.Articles.AsNoTracking()
               .Where(x => x.Slug == slug)
               .Select(x => x.Id)
               .FirstOrDefault();

            var dbUser = _dbContext.Users.AsNoTracking()
                .Where(x => x.Username == Username)
                .FirstOrDefault();

            if (dbArticleId == 0 || dbUser == null)
            {
                throw NotFoundException();
            }

            var dbComment = new Data.Comment
            {
                ArticleId = dbArticleId,
                AuthorId = dbUser.Id,
                Body = comment.Content.Body,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Comments.Add(dbComment);
            _dbContext.SaveChanges();

            return new Envelope<Comment>
            {
                EnvelopePropertyName = "comment",
                Content = new Comment
                {
                    Author = new Profile
                    {
                        Bio = dbUser.Bio,
                        Following = false,
                        Image = dbUser.Image,
                        Username = dbUser.Username
                    },
                    Body = dbComment.Body,
                    CreatedAt = dbComment.CreatedAt,
                    Id = dbComment.CommentId,
                    UpdatedAt = dbComment.UpdatedAt
                }
            };
        }

        [HttpGet]
        [Route("articles/{slug}/comments")]
        public Envelope<Comment[]> GetComments(string slug)
        {
            if(!_dbContext.Articles.Any(x=>x.Slug == slug))
            {
                throw NotFoundException();
            }
            else
            {
                var result = new Envelope<Comment[]>
                {
                    EnvelopePropertyName = "comments",
                    Content = _dbContext.Comments.Where(c => c.Article.Slug == slug)
                    .Select(x => new Comment
                    {
                        Author = new Profile
                        {
                            Bio = x.Author.Bio,
                            Following = Username != null && x.Author.Followers.Any(f=>f.FollowingUser.Username == Username),
                            Image = x.Author.Image,
                            Username = x.Author.Username
                        },
                        Body = x.Body,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        Id = x.CommentId
                    }).ToArray()
                };
                result.Count = result.Count;
                return result;
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("articles/{slug}/comments/{id}")]
        public void DeleteComment(string slug, int id)
        {
            var dbComment = _dbContext.Comments.FirstOrDefault(c => c.Article.Slug == slug && c.CommentId == id);

            if(dbComment == null)
            {
                throw NotFoundException();
            }
            else
            {
                _dbContext.Comments.Remove(dbComment);
                _dbContext.SaveChanges();
            }
        }
    }
}
