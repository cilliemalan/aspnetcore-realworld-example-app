using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class Login
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Password { get; set; }
    }
}
