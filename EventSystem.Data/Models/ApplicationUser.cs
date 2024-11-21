using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public virtual ICollection<UserEvent> UsersEvents { get; set; } = new HashSet<UserEvent>();
    }
}
