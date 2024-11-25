using EventSystem.Data.Enum;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data.Models
{
    public class EventInvitation
    {
        public EventInvitation()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        [Required]
        public Guid CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public ApplicationUser Creator { get; set; } = null!;

        [Required]
        public Guid InvitedPersonId { get; set; }
        [ForeignKey(nameof(InvitedPersonId))]
        public ApplicationUser InvitedPerson { get; set; } = null!;

        [Required]
        public Guid EventId { get; set; }
        [ForeignKey(nameof(EventId))]

        public Event Event { get; set; } = null!;

        [Required]
        public DateTime InviteDate { get; set; }

        [Required]
        public InvitationStatus InvitationStatus { get; set; }

        [Required]
        public string EventName { get; set; } = null!;
    }
}
