﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.AdminViewModel
{
    public class RecentlyDeletedEventsViewModel
    {
        public required Guid EventId { get; set; }
        public required string EventName { get; set; }
        public required string EventDescription { get; set; }
        public required string EventLocation { get; set; }
    }
}