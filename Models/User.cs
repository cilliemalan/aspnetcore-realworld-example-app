using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
    }
}
