using ConduitApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class ProfileController : ApiController
    {
        private readonly Data.ConduitDbContext _dbContext;

        public ProfileController(Data.ConduitDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("profiles/{username}")]
        public Envelope<Profile> GetProfile(string username)
        {
            var user = _dbContext.Users.Where(u => u.Username == username)
                .Select(u => new Profile
                {
                    Bio = u.Bio,
                    Following = Username != null ? u.Followers.Any(f => f.FollowingUser.Username == Username) : false,
                    Image = u.Image,
                    Username = u.Username,
                }).FirstOrDefault();

            if (user == null)
            {
                throw NotFoundException();
            }
            else
            {
                return new Envelope<Profile>
                {
                    EnvelopePropertyName = "profile",
                    Content = user
                };
            }
        }

        [Authorize]
        [HttpPost]
        [Route("profiles/{username}/follow")]
        public Envelope<Profile> Follow(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                ModelState.AddModelError("username", "Username is invalid.");
            }
            else if (string.Equals(username, Username, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("username", "You cannot follow yourself.");
            }
            EnsureModelValid();

            var user = _dbContext.Users.Where(u => u.Username == username)
                .Select(u => new Profile
                {
                    Bio = u.Bio,
                    Following = Username != null ? u.Followers.Any(f => f.FollowingUser.Username == Username) : false,
                    Image = u.Image,
                    Username = u.Username,
                }).FirstOrDefault();

            if (user == null)
            {
                throw NotFoundException();
            }
            else
            {
                if (!user.Following)
                {
                    _dbContext.Follows.Add(new Data.Follow
                    {
                        FollowingUserId = _dbContext.Users.Where(u => u.Username == Username).Select(u => u.Id).Single(),
                        FollowedUserId = _dbContext.Users.Where(u => u.Username == username).Select(u => u.Id).Single()
                    });
                    _dbContext.SaveChanges();
                    user.Following = true;
                }

                return new Envelope<Profile>
                {
                    EnvelopePropertyName = "profile",
                    Content = user
                };
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("profiles/{username}/follow")]
        public Envelope<Profile> UnFollow(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                ModelState.AddModelError("username", "Username is invalid.");
            }
            else if (string.Equals(username, Username, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("username", "You cannot follow yourself.");
            }
            EnsureModelValid();

            var user = _dbContext.Users.Where(u => u.Username == username)
                .Select(u => new Profile
                {
                    Bio = u.Bio,
                    Following = Username != null ? u.Followers.Any(f => f.FollowingUser.Username == Username) : false,
                    Image = u.Image,
                    Username = u.Username,
                }).FirstOrDefault();

            if (user == null)
            {
                throw NotFoundException();
            }
            else
            {
                if (user.Following)
                {
                    _dbContext.Follows.RemoveRange(_dbContext.Follows.Where(f => f.FollowedUser.Username == username && f.FollowingUser.Username == Username));
                    _dbContext.SaveChanges();
                    user.Following = false;
                }

                return new Envelope<Profile>
                {
                    EnvelopePropertyName = "profile",
                    Content = user
                };
            }
        }
    }
}
