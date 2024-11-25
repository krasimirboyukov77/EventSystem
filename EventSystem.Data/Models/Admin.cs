using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        [ForeignKey(nameof(UserId))]

        public virtual ApplicationUser User { get; set; } = null!;


    }
}
