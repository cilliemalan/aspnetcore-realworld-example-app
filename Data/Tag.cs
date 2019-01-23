using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class Tag
    {
        public int ArticleId { get; set; }
        
        [MaxLength(100)]
        public string TagName { get; set; }

        public virtual Article Article { get; set; }
    }
}
