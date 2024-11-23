using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data.Models
{
    public class Admin
    {

        public Guid Id { get; set; }  
        public Guid UserId { get; set; }  
        public string? SpecialPermissions { get; set; }

        public virtual ApplicationUser User { get; set; }


    }
}
