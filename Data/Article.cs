using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class Article
    {
        public int Id { get; set; }

        [MaxLength(250)]
        [Required]
        public string Title { get; set; }

        [MaxLength(100)]
        [Required]
        public string Slug { get; set; }

        public int AuthorId { get; set; }

        public string Description { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        
        public virtual User Author { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }
    }
}
