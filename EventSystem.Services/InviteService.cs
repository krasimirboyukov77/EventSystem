using EventSystem.Data.Models;
using EventSystem.Repositories.Interfaces;
using EventSystem.Services.Interfaces;
using EventSystem.ViewModels.EventViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace EventSystem.Services
{
    public class InviteService : BaseService, IInviteService
    {
        private readonly IRepository<EventInvitation> _inviteRepository;
        private readonly IRepository<UserEvent> _userEventRepository;
        private readonly IRepository<Event> _eventRepository;

        public InviteService(
            IRepository<EventInvitation> inviteRepository
            , IHttpContextAccessor httpContextAccessor
            , IRepository<UserEvent> userEventRepository
            ,IRepository<Event> eventRepository
            ,UserManager<ApplicationUser> userManager) : base(httpContextAccessor, userManager)
        {
            _inviteRepository = inviteRepository;
            _userEventRepository = userEventRepository;
            _eventRepository = eventRepository;
        }

        public async Task<bool> Accept(string inviteId)
        {
            var isEventIdValid = Guid.TryParse(inviteId, out var inviteGuid);

            if (!isEventIdValid)
            {
                return false;
            }

            var entity = await _inviteRepository.FirstOrDefaultAsync(i => i.Id == inviteGuid);

            if (entity == null)
            {
                return false;
            }

            entity.InvitationStatus = Data.Enum.InvitationStatus.Accepted;

            UserEvent userEvent = new UserEvent()
            {
                EventId = entity.EventId,
                UserId = GetUserId(),
                AttendStatus = Data.Enum.AttendStatus.Attending
            };

            await _userEventRepository.AddAsync(userEvent);
           
            return true;
        }

        public async Task<bool> Decline(string inviteId)
        {
            var isEventIdValid = Guid.TryParse(inviteId, out var inviteGuid);

            if (!isEventIdValid)
            {
                return false;
            }

            var entity = await _inviteRepository.FirstOrDefaultAsync(i => i.Id == inviteGuid);

            if (entity == null)
            {
                return false;
            }

            entity.InvitationStatus = Data.Enum.InvitationStatus.Declined;
            await _inviteRepository.UpdateAsync(entity);

            return true;
        }

        public async Task<InviteViewModel?> GetInvites()
        {
            var invitations = await _inviteRepository.GetAllAttached()
               .Include(i => i.Creator)
               .Where(i => i.InvitedPersonId == GetUserId())
               .Select(i => new InviteInfo()
               {
                   Id = i.Id.ToString(),
                   EventName = i.EventName,
                   InviteDate = i.InviteDate,
                   CreatorName = i.Creator.UserName ?? string.Empty,
                   InvitationStatus = (int)i.InvitationStatus
               })
               .ToListAsync();

            InviteViewModel viewModel = new InviteViewModel();

            foreach (var inv in invitations)
            {
                if (inv.InvitationStatus == 0)
                {
                    viewModel.PendingInvites.Add(inv);
                }
                else
                {
                    viewModel.AnsweredInvites.Add(inv);
                }
            }

            var sentInvites = await _inviteRepository.GetAllAttached()
                .Where(i => i.CreatorId == GetUserId())
                .Select(i => new InviteInfo()
                {
                    Id = i.Id.ToString(),
                    EventName = i.EventName,
                    InviteDate = i.InviteDate,
                    CreatorName = i.Creator.UserName ?? string.Empty,
                    InvitationStatus = (int)i.InvitationStatus,
                    IsSentByMe = true
                })
                .ToListAsync();

            viewModel.SentInvites = sentInvites;

            return viewModel;
        }

        public async Task<bool> InviteUser(string userId, string eventId)
        {

            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return false;
            }

            var eventForInvite = await _eventRepository.FirstOrDefaultAsync(e => e.id == eventGuid);

            if (eventForInvite == null)
            {
                return false;
            }

            bool isUserGuidValid = Guid.TryParse(userId, out var userGuid);

            if (!isUserGuidValid)
            {
                return false;
            }

            var isUserInvitedToEvent = await _userEventRepository
                .FirstOrDefaultAsync(e => e.UserId == userGuid && e.EventId == eventGuid);

            if (isUserInvitedToEvent != null)
            {
                return false;
            }

            EventInvitation invitation = new EventInvitation()
            {
                CreatorId = GetUserId(),
                InvitedPersonId = userGuid,
                EventId = eventGuid,
                InviteDate = eventForInvite.Date,
                InvitationStatus = Data.Enum.InvitationStatus.Pending,
                EventName = eventForInvite.Name
            };

            await _inviteRepository.AddAsync(invitation);

            return true;
        }
    }
}
