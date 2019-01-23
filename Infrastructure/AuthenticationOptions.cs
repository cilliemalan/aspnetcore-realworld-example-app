using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    public class AuthenticationOptions : AuthenticationSchemeOptions
    {
        public static readonly string DefaultScheme = "Basic Token Authentication";

        public string Scheme { get; set; } = DefaultScheme;

        public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromDays(10);
    }
}
