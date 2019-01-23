using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Data
{
    public class Follow
    {
        [ForeignKey(nameof(FollowingUser))]
        public int FollowingUserId { get; set; }
        
        [ForeignKey(nameof(FollowedUser))]
        public int FollowedUserId { get; set; }

        public virtual User FollowingUser { get; set; }

        public virtual User FollowedUser { get; set; }
    }
}
