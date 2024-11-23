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
}
