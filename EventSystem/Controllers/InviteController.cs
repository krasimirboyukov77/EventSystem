using EventSystem.Data;
using EventSystem.Data.Models;
using EventSystem.ViewModels.EventViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EventSystem.Controllers
{
    [Authorize]
    public class InviteController : Controller
    {

        private readonly ApplicationDbContext _context;

        public InviteController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var invitations = await _context.Invites
                .Include(i=> i.Creator)
                .Where(i => i.InvitedPersonId == GetUserId())
                .Select( i => new InviteInfo()
                {
                    Id = i.Id.ToString(),
                    EventName = i.EventName,
                    InviteDate = i.InviteDate,
                    CreatorName = i.Creator.UserName ?? string.Empty,
                    InvitationStatus = (int)i.InvitationStatus
                })
                .ToListAsync();

            InviteViewModel viewModel = new InviteViewModel();

            foreach(var inv in invitations)
            {
                if(inv.InvitationStatus == 0)
                {
                    viewModel.PendingInvites.Add(inv);
                }
                else
                {
                    viewModel.AnsweredInvites.Add(inv);
                }
            }

            var sentInvites = await _context.Invites.Where(i => i.CreatorId == GetUserId())
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

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> InviteUser(string userId, string eventId)
        {
            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return NotFound();
            }

            var eventForInvite = await _context.Events.FirstOrDefaultAsync(e => e.id == eventGuid);

            if (eventForInvite == null)
            {
                return NotFound();
            }

            bool isUserGuidValid = Guid.TryParse(userId, out var userGuid);

            if (!isUserGuidValid)
            {
                return NotFound();
            }

            var isUserInvitedToEvent = await _context.UsersEvents.FirstOrDefaultAsync(e => e.UserId == userGuid && e.EventId == eventGuid);

            if (isUserInvitedToEvent != null)
            {
                return NotFound();
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

            await _context.Invites.AddAsync(invitation);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string inviteId)
        {
            var isEventIdValid = Guid.TryParse(inviteId, out var inviteGuid);

            if (!isEventIdValid)
            {
                return NotFound();
            }

            var entity = await _context.Invites.FirstOrDefaultAsync(i=> i.Id == inviteGuid);

            if (entity == null)
            {
                return NotFound();
            }

            entity.InvitationStatus = Data.Enum.InvitationStatus.Accepted;

            UserEvent userEvent = new UserEvent()
            {
                EventId = entity.EventId,
                UserId = GetUserId(),
                AttendStatus = Data.Enum.AttendStatus.Attending
            };

            await _context.UsersEvents.AddAsync(userEvent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Decline(string inviteId)
        {
            var isEventIdValid = Guid.TryParse(inviteId, out var inviteGuid);

            if (!isEventIdValid)
            {
                return NotFound();
            }

            var entity = await _context.Invites.FirstOrDefaultAsync(i => i.Id == inviteGuid);

            if (entity == null)
            {
                return NotFound();
            }

            entity.InvitationStatus = Data.Enum.InvitationStatus.Declined;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Guid.Empty;
            }

            return Guid.Parse(userId);
        }
    }
}
