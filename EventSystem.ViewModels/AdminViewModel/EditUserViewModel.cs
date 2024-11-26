using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.AdminViewModel
{
    public class EditUserViewModel
    {
        public required Guid UserId { get; set; }

        [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters.", MinimumLength = 3)]
        public required string UserName { get; set; }

        [EmailAddress]
        public required string UserEmail { get; set; }

        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        public required string NewPassword { get; set; }

    }
}
