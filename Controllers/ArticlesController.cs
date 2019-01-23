using ConduitApi.Infrastructure;
using ConduitApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class ArticlesController : ApiController
    {
        private readonly Data.ConduitDbContext _dbContext;
        private readonly Authentication _authentication;

        public ArticlesController(Data.ConduitDbContext dbContext, Authentication authentication)
        {
            _dbContext = dbContext;
            _authentication = authentication;
        }

        [HttpGet]
        [Route("articles")]
        public Envelope<Article[]> GetArticles(string tag, string author, string favorited, int? limit = 20, int? offset = 0)
        {
            IQueryable<Data.Article> articles = _dbContext.Articles
                .Include(x => x.Author)
                .OrderByDescending(x => x.CreatedAt);

            if (!string.IsNullOrEmpty(tag)) articles = articles.Where(x => x.Tags.Any(t => t.TagName == tag));
            if (!string.IsNullOrEmpty(author)) articles = articles.Where(x => x.Author.Username == author);
            if (!string.IsNullOrEmpty(favorited)) articles = articles.Where(x => x.Favorites.Any(f => f.User.Username == favorited));
            var count = articles.Count();
            if (offset.HasValue) articles = articles.Skip(offset.Value);
            if (limit.HasValue) articles = articles.Take(limit.Value);

            return ArticlesFromDb(articles, count);
        }

        [Authorize]
        [HttpGet]
        [Route("articles/feed")]
        public Envelope<Article[]> Feed()
        {
            IQueryable<Data.Article> articles = _dbContext.Articles
                .Include(x => x.Author)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.Author.Followers.Any(f => f.FollowingUser.Username == Username));

            return ArticlesFromDb(articles, articles.Count());
        }

        [Authorize]
        [HttpPost]
        [Route("articles")]
        public Envelope<Article> CreateArticle([FromBody]Envelope<NewArticle> articleEnvelope)
        {
            EnsureModelValid();

            var article = articleEnvelope.Content;
            if (article.TagList.Any(string.IsNullOrEmpty))
            {
                ModelState.AddModelError("TagList", "There were blank tags in the tag list");
            }
            if (article.TagList.Any(x => x.Length > 100))
            {
                ModelState.AddModelError("TagList", "Some of the tags were too long");
            }

            EnsureModelValid();

            string slug = GenerateSlug(article.Title);
            var author = _dbContext.Users.Single(u => u.Username == Username);
            Data.Tag[] dbTags = article.TagList.Distinct(StringComparer.OrdinalIgnoreCase).Select(t => new Data.Tag { TagName = t }).ToArray();
            var dbArticle = new Data.Article
            {
                Author = author,
                Body = article.Body,
                CreatedAt = DateTime.UtcNow,
                Description = article.Description,
                Slug = slug,
                Title = article.Title,
                UpdatedAt = DateTime.UtcNow,
                Tags = dbTags
            };
            _dbContext.Articles.Add(dbArticle);
            _dbContext.SaveChanges();

            return new Envelope<Article>
            {
                EnvelopePropertyName = "article",
                Content = new Article
                {
                    Author = new Profile
                    {
                        Bio = author.Bio,
                        Following = false,
                        Image = author.Image,
                        Username = author.Username
                    },
                    Body = dbArticle.Body,
                    CreatedAt = dbArticle.CreatedAt,
                    Description = dbArticle.Description,
                    Favorited = false,
                    FavoritesCount = 0,
                    Slug = dbArticle.Slug,
                    TagList = dbArticle.Tags.Select(z => z.TagName).ToArray(),
                    Title = dbArticle.Title,
                    UpdatedAt = dbArticle.UpdatedAt == dbArticle.CreatedAt ? null : (DateTime?)dbArticle.UpdatedAt
                }
            };
        }

        [Authorize]
        [HttpPut("articles/{slug}")]
        public Envelope<Article> UpdateArticle(string slug, [FromBody]Envelope<UpdateArticle> articleEnvelope)
        {
            var article = articleEnvelope.Content;
            if (article.TagList != null)
            {
                if (article.TagList.Any(string.IsNullOrEmpty))
                {
                    ModelState.AddModelError("TagList", "There were blank tags in the tag list");
                }
                if (article.TagList.Any(x => x.Length > 100))
                {
                    ModelState.AddModelError("TagList", "Some of the tags were too long");
                }
            }

            EnsureModelValid();

            var dbArticle = _dbContext.Articles
                .Include(x => x.Tags)
                .Include(x => x.Author)
                .Where(x => x.Slug == slug && x.Author.Username == Username)
                .FirstOrDefault();

            var newTitle = article.Title;
            var newBody = article.Body;
            var newDescription = article.Description;
            var newTags = article.TagList;

            if (!string.IsNullOrEmpty(newTitle) && !string.Equals(newTitle, dbArticle.Title, StringComparison.Ordinal))
            {
                var newSlug = dbArticle.Slug;
                if (!string.Equals(GenerateSlugSimple(newTitle), GenerateSlugSimple(dbArticle.Title), StringComparison.Ordinal))
                {
                    newSlug = GenerateSlug(newTitle);
                }

                dbArticle.Slug = newSlug;
                dbArticle.Title = newTitle;
            }

            if (!string.IsNullOrEmpty(newBody)) dbArticle.Body = newBody;
            if (!string.IsNullOrEmpty(newDescription)) dbArticle.Description = newDescription;

            if (newTags != null)
            {
                var tagsToAdd = newTags.Where(nt => !dbArticle.Tags.Any(ot => string.Equals(nt, ot.TagName, StringComparison.OrdinalIgnoreCase)))
                    .Select(nt => new Data.Tag { TagName = nt })
                    .ToArray();
                var tagsToRemove = dbArticle.Tags.Where(ot => !newTags.Any(nt => string.Equals(nt, ot.TagName, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
                foreach (var tag in tagsToAdd) dbArticle.Tags.Add(tag);
                foreach (var tag in tagsToRemove) dbArticle.Tags.Remove(tag);
            }

            _dbContext.SaveChanges();

            // ArticlesFromDb already does a lot of magic, so use it
            return new Envelope<Article>
            {
                EnvelopePropertyName = "article",
                Content = new Article
                {
                    Author = new Profile
                    {
                        Bio = dbArticle.Author.Bio,
                        Following = false,
                        Image = dbArticle.Author.Image,
                        Username = dbArticle.Author.Username
                    },
                    Body = dbArticle.Body,
                    CreatedAt = dbArticle.CreatedAt,
                    Description = dbArticle.Description,
                    Favorited = false,
                    FavoritesCount = _dbContext.Favorites.Where(f=>f.ArticleId == dbArticle.Id).Count(),
                    Slug = dbArticle.Slug,
                    TagList = dbArticle.Tags.Select(t => t.TagName).ToArray(),
                    Title = dbArticle.Title,
                    UpdatedAt = dbArticle.UpdatedAt == dbArticle.CreatedAt ? null : (DateTime?)dbArticle.UpdatedAt
                }
            };
        }

        [HttpGet]
        [Route("articles/{slug}")]
        public Envelope<Article> GetArticle(string slug)
        {
            var dbArticle = _dbContext.Articles
                .Include(x => x.Tags)
                .Include(x => x.Author)
                .Where(x => x.Slug == slug);

            // ArticlesFromDb already does a lot of magic, so use it
            var dummy = ArticlesFromDb(dbArticle, 0);
            if (dummy.Content.Length == 0) throw NotFoundException();
            else return new Envelope<Article>
            {
                EnvelopePropertyName = "article",
                Content = dummy.Content[0]
            };
        }

        [Authorize]
        [HttpDelete]
        [Route("articles/{slug}")]
        public void DeleteArticle(string slug)
        {
            var dbArticleId = _dbContext.Articles.AsNoTracking()
                .Where(x => x.Slug == slug && x.Author.Username == Username)
                .Select(x => x.Id)
                .FirstOrDefault();

            if (dbArticleId == 0)
            {
                throw NotFoundException();
            }
            else
            {
                _dbContext.DeleteArticle(dbArticleId);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("articles/{slug}/favorite")]
        public Envelope<Article> FavoriteArticle(string slug)
        {
            var dbArticleId = _dbContext.Articles.AsNoTracking()
                .Where(x => x.Slug == slug)
                .Select(x => x.Id)
                .FirstOrDefault();

            var dbUserId = _dbContext.Users.AsNoTracking()
                .Where(x => x.Username == Username)
                .Select(x => x.Id)
                .FirstOrDefault();

            if (dbArticleId == 0 || dbUserId == 0)
            {
                throw NotFoundException();
            }
            else
            {
                _dbContext.Favorites.Add(new Data.Favorite
                {
                    ArticleId = dbArticleId,
                    UserId = dbUserId
                });
                _dbContext.SaveChanges();

                return GetArticle(slug);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("articles/{slug}/favorite")]
        public Envelope<Article> UnfavoriteArticle(string slug)
        {
            var dbFavorite = _dbContext.Favorites
                .Where(x => x.Article.Slug == slug && x.User.Username == Username)
                .FirstOrDefault();

            if (dbFavorite != null)
            {
                _dbContext.Favorites.Remove(dbFavorite);
                _dbContext.SaveChanges();
            }

            return GetArticle(slug);
        }

        private Envelope<Article[]> ArticlesFromDb(IQueryable<Data.Article> articles, int articlesCount)
        {
            return new Envelope<Article[]>
            {
                Count = articlesCount,
                EnvelopePropertyName = "articles",
                Content = articles.Select(x => new Article
                {
                    Author = new Profile
                    {
                        Bio = x.Author.Bio,
                        Following = Username != null ? x.Author.Followers.Any(f => f.FollowingUser.Username == Username) : false,
                        Image = x.Author.Image,
                        Username = x.Author.Username
                    },
                    Body = x.Body,
                    CreatedAt = x.CreatedAt,
                    Description = x.Description,
                    Favorited = Username != null ? x.Favorites.Any(f => f.User.Username == Username) : false,
                    FavoritesCount = x.Favorites.Count,
                    Slug = x.Slug,
                    TagList = x.Tags.Select(t => t.TagName).ToArray(),
                    Title = x.Title,
                    UpdatedAt = x.UpdatedAt == x.CreatedAt ? null : (DateTime?)x.UpdatedAt
                }).ToArray()
            };
        }

        private string GenerateSlug(string title)
        {
            string slug = GenerateSlugSimple(title);

            var existingSimilarTitle = _dbContext.Articles.Where(x => x.Slug.StartsWith(slug) && x.Slug.Length > slug.Length + 1)
                .Select(x => x.Slug.Substring(slug.Length + 1))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (existingSimilarTitle != null)
            {
                if (int.TryParse(existingSimilarTitle, out var nr))
                {
                    slug = $"{slug}-{nr + 1}";
                }
                else if (existingSimilarTitle == "")
                {
                    slug = $"{slug}-2";
                }
                else
                {
                    throw new InvalidOperationException("Could not generate slug for article becuase database contained messed up other slugs");
                }
            }

            return slug;
        }

        private static string GenerateSlugSimple(string title)
        {
            var slug = Regex.Replace(title, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-zA-Z0-9-]", "");
            slug = slug.ToLowerInvariant();
            if (slug.Length > 90) slug = slug.Substring(0, 90);
            return slug;
        }
    }
}
