using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public virtual ICollection<UserEvent> UsersEvents { get; set; } = new HashSet<UserEvent>(); 
    }
}
