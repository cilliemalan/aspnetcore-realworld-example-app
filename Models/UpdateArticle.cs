using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class UpdateArticle
    {
        [MinLength(10)]
        [MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(5000)]
        public string Description { get; set; }

        [Required]
        public string Body { get; set; }

        public string[] TagList { get; set; }
    }
}
