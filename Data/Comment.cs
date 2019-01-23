using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class Comment
    {
        public int ArticleId { get; set; }

        public int CommentId { get; set; }

        public int? AuthorId { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        
        public virtual User Author { get; set; }
        
        public virtual Article Article { get; set; }
    }
}
