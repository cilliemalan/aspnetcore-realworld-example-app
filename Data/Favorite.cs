using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class Favorite
    {
        public int ArticleId { get; set; }
        
        public int UserId { get; set; }
        
        public virtual Article Article { get; set; }
        
        public virtual User User { get; set; }
    }
}
