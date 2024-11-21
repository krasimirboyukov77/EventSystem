using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data.Models
{
    public class Event
    {
        public Guid id {  get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]  
        public string Description { get; set; } = null!;
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Location { get; set; } = null!;

        public virtual ICollection<UserEvent> UsersEvents { get; set; } = new HashSet<UserEvent>(); 
    }
}
