using ConduitApi.Infrastructure;
using ConduitApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class UsersController : ApiController
    {
        private readonly Data.ConduitDbContext _dbContext;
        private readonly Authentication _authentication;

        public UsersController(Data.ConduitDbContext dbContext, Authentication authentication)
        {
            _dbContext = dbContext;
            _authentication = authentication;
        }

        [HttpPost]
        [Route("users")]
        public Envelope<User> Register([FromBody]Envelope<Registration> registrationEnvelope)
        {
            EnsureModelValid();
            var login = registrationEnvelope.Content;

            // make sure the user doesn't already exist.
            if (_dbContext.Users.Any(x => x.Username == login.Username))
            {
                ModelState.AddModelError("username", "A user with the same username already exists");
            }
            if (_dbContext.Users.Any(x => x.Email == login.Email))
            {
                ModelState.AddModelError("username", "A user with the same email address already exists");
            }

            EnsureModelValid();

            Data.User dbUser = new Data.User
            {
                Email = login.Email,
                Username = login.Username,
                Password = _authentication.HashPassword(login.Password)
            };
            _dbContext.Users.Add(dbUser);
            _dbContext.SaveChanges();

            var token = _authentication.GenerateToken(new MinimalUser
            {
                Username = login.Username,
                Created = DateTime.UtcNow
            });

            return UserFromDbUser(dbUser, token);
        }

        [Authorize]
        [HttpGet]
        [Route("user")]
        public Envelope<User> GetCurrentUser()
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            return UserFromDbUser(user);
        }

        [Authorize]
        [HttpPut]
        [Route("user")]
        public Envelope<User> UpdateUser([FromBody]Envelope<UserUpdate> user)
        {
            EnsureModelValid();

            var newEmail = user.Content.Email;
            var newBio = user.Content.Bio;
            var newImage = user.Content.Image;
            var dbUser = _dbContext.Users.FirstOrDefault(u => u.Username == User.Identity.Name);

            if (!string.IsNullOrEmpty(newEmail) && !string.Equals(newEmail, dbUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (_dbContext.Users.Any(u => u.Email == newEmail && u.Id != dbUser.Id))
                {
                    ModelState.AddModelError("email", "The email is already in use");
                }
                else
                {
                    dbUser.Email = newEmail;
                }
            }

            if (!string.IsNullOrEmpty(newImage))
            {
                dbUser.Image = newImage;
            }

            if (!string.IsNullOrEmpty(newBio))
            {
                dbUser.Bio = newBio;
            }

            EnsureModelValid();

            _dbContext.SaveChanges();
            
            return UserFromDbUser(dbUser);
        }

        [HttpPost]
        [Route("users/login")]
        public Envelope<User> Login([FromBody]Envelope<Login> login)
        {
            EnsureModelValid();

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == login.Content.Email);
            var passwordValid = _authentication.VerifyPassword(login.Content.Password, user?.Password);

            if (!passwordValid)
            {
                ModelState.AddModelError("", "Username or Password invalid");
            }

            EnsureModelValid();

            string token = _authentication.GenerateToken(new MinimalUser { Created = DateTime.Now, Username = user.Username });
            return UserFromDbUser(user, token);
        }

        private Envelope<User> UserFromDbUser(Data.User user, string token = null) =>
            new Envelope<User>
            {
                EnvelopePropertyName = "user",
                Content = new User
                {
                    Bio = user.Bio,
                    Email = user.Email,
                    Image = user.Image,
                    Token = token ?? GetTokenFromHeader(),
                    Username = user.Username
                }
            };

        private string GetTokenFromHeader() =>
            Request.Headers["Authorize"].Count > 0 ? Regex.Match(Request.Headers["Authorize"][0], @"^(?:Bearer|Token) (.+)$")?.Groups[1]?.Value : null;
    }
}
