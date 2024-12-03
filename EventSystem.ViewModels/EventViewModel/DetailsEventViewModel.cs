using EventSystem.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.EventViewModel
{
    public class DetailsEventViewModel
    {
        public required Guid id { get; set; }
        public required string Name { get; set; } = null!;
        public required string Description { get; set; } = null!;
        public required DateTime Date { get; set; }
        public required string LocationName { get; set; } = null!;

        public required string HostName { get; set; }
        public Guid HostId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int Atendees { get; set; }

        public bool IsAttending { get; set; }

        public virtual List<PersonInfo> PopleAttending { get; set; } = new List<PersonInfo>();
    }
}
