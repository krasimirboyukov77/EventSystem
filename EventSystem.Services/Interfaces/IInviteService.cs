using EventSystem.ViewModels.EventViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Services.Interfaces
{
    public interface IInviteService
    {
        Task<InviteViewModel?> GetInvites();
        Task<bool> InviteUser(string userId, string eventId);
        Task<bool> Accept(string inviteId);
        Task<bool> Decline(string inviteId);
    }
}
