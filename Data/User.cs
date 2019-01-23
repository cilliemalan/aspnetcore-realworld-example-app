using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Username { get; set; }

        [MaxLength(100)]
        [Required]
        public string Email { get; set; }

        public string Bio { get; set; }

        [MaxLength(256)]
        public string Image { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        public virtual ICollection<Article> Articles { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }

        public virtual ICollection<Follow> Followings { get; set; }

        public virtual ICollection<Follow> Followers { get; set; }
    }
}
