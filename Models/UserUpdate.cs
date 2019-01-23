using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class UserUpdate
    {
        [MaxLength(100)]
        [MinLength(5)]
        public string Email { get; set; }
        
        [MaxLength(5000)]
        public string Bio { get; set; }

        [MaxLength(200)]
        public string Image { get; set; }
    }
}
