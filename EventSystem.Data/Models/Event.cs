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
        //Това е за да можем от админският профил да виждаме всички евенти и от кой са създадени
        public Guid CreatedById { get; set; }  // UserId of the creator
        public virtual ApplicationUser CreatedBy { get; set; }  // Navigation property to the creator user
        public virtual ICollection<UserEvent> UsersEvents { get; set; } = new HashSet<UserEvent>(); 
    }
}
