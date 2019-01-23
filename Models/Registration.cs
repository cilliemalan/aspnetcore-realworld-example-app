using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class Registration
    {
        [Required]
        [MaxLength(100)]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        public string Email { get; set; }

        [Required]
        [MaxLength(30)]
        [MinLength(7)]
        public string Password { get; set; }
    }
}
