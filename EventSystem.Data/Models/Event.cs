using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventSystem.Common;

namespace EventSystem.Data.Models
{
    public class Event
    {
        public Event()
        {
            this.id = Guid.NewGuid();
        }

        [Required]
        public Guid id {  get; set; }
        [Required]
        [MaxLength(EventValidationConstants.EventNameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(EventValidationConstants.EventDescriptionMaxLength)]
        public string Description { get; set; } = null!;
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(EventValidationConstants.EventLocationMaxLength)]
        public string Location { get; set; } = null!;

        [Required]
        public Guid HostId { get; set; }
        [ForeignKey(nameof(HostId))]
        public ApplicationUser Host {  get; set; }
        [Required]
        public bool IsDeleted { get; set; }

        public virtual ICollection<UserEvent> UsersEvents { get; set; } = new HashSet<UserEvent>(); 
    }
}
