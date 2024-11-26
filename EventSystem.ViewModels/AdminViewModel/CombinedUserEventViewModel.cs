using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.AdminViewModel
{
    public class CombinedUserEventViewModel
    {
        public required List<UserEventsViewModel> EventDetails { get; set; }
        public required List<RegisteredUsersViewModel> RegisteredUsers { get; set; }
        public required List<RecentlyDeletedEventsViewModel> RecentlyDeletedEvents { get; set; }
    }
}
