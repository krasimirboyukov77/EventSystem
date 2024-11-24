using EventSystem.ViewModels.EventViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.AdminViewModel
{
    public class DetailsAdminViewModel
    {
        public required Guid Eventid { get; set; }
        public required string EventName { get; set; } = null!;
        public required string EventDescription { get; set; } = null!;
        public required DateTime EventDate { get; set; }
        public required string EventLocation { get; set; } = null!;

        public virtual List<PersonInfo> PopleAttending { get; set; } = new List<PersonInfo>();

        public required Guid Userid { get; set; }
        public required string UserEmail { get; set; }
    }
}
