using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.StatisticsViewModels
{
    public class StatisticsViewModel
    {
        public int EventsCreated { get; set; }
        public int PeopleInvited { get; set; }
        public double PeopleAcceptedPercent { get; set; }
    }
}
