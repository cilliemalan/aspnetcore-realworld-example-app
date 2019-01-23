using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Models
{
    public class NewComment
    {
        [Required]
        [MinLength(3)]
        [MaxLength(5000)]
        public string Body { get; set; }
    }
}
