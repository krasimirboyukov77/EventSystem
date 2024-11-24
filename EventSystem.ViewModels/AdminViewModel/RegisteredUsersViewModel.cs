using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.AdminViewModel
{
    public class RegisteredUsersViewModel
    {
        public required Guid UserId { get; set; }
        public required string UserEmail { get; set; }
        public required string UserName { get; set; }
    }
}
