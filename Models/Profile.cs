using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class Profile
    {
        public string Username { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public bool Following { get; set; }
    }
}
