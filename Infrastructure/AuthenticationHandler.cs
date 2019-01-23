using ConduitApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationMiddleware>();
        }
    }

    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddBasicTokenAuthentication(this AuthenticationBuilder builder) =>
            AddBasicTokenAuthentication(builder, _ => { });

        public static AuthenticationBuilder AddBasicTokenAuthentication(this AuthenticationBuilder builder, Action<AuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<AuthenticationOptions, AuthenticationHandler>(AuthenticationOptions.DefaultScheme, configureOptions);
        }
    }

    public class AuthenticationHandler : AuthenticationHandler<AuthenticationOptions>
    {
        private Authentication _auth;

        public AuthenticationHandler(IOptionsMonitor<AuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, Authentication authentication)
            : base(options, logger, encoder, clock)
        {
            _auth = authentication;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeaders) || authHeaders.Count != 1)
            {
                return Task.FromResult(AuthenticateResult.Fail("Could not read authorization header."));
            }
            
            var authHeader = authHeaders[0];
            var match = Regex.Match(authHeader, @"^(?:Bearer|Token) (.+)$");
            if (match.Success)
            {
                var token = match.Groups[1].Value;
                if (_auth.VerifyToken(token))
                {
                    var user = _auth.DeserializeToken(token);
                    if (user?.Username != null)
                    {
                        if (DateTime.UtcNow - user.Created < Options.TokenExpiry)
                        {
                            var ticket = new AuthenticationTicket(GetClaimsPrincipalFor(user), Options.Scheme);
                            var result = AuthenticateResult.Success(ticket);
                            return Task.FromResult(result);
                        }
                        else
                        {
                            return Task.FromResult(AuthenticateResult.Fail("The token is expired."));
                        }
                    }
                    else
                    {
                        return Task.FromResult(AuthenticateResult.Fail("Could not read token."));
                    }
                }
                else
                {
                    return Task.FromResult(AuthenticateResult.Fail("Could not verify token."));
                }
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("Could not read authorization token."));
            }
        }

        private ClaimsPrincipal GetClaimsPrincipalFor(MinimalUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.NameIdentifier, user.Username)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "bearer", ClaimTypes.Name, ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }
    }
}
