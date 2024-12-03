using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.EventViewModel
{
    public class PersonInfo
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;
        public int AttendStatus { get; set; }
    }

    public class UserList
    {
        public string EventId { get; set; } = null!;
        public string HostId { get; set; } = null!;

        public List<PersonName> People { get; set; } = new List<PersonName>();
    }
    public class PersonName 
    {
        public string Name { get; set; } = null!;
        public string? Id { get; set; }
    }

}
