using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data.Models
{
    public class UserEvent
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public Guid EventId { get; set; }
        [Required]
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

    }
}
