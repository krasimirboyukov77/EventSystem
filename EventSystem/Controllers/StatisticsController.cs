using EventSystem.Data;
using EventSystem.Data.Enum;
using EventSystem.ViewModels.StatisticsViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventSystem.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var eventsCreated = await _context.Events.Where(e => e.HostId == GetUserId()).CountAsync();

            var totalPeopleInvited = await _context.Invites.Where(i=> i.CreatorId == GetUserId()).CountAsync();

            var peopleAccepted = await _context.Invites.Where(i=> i.CreatorId == GetUserId() && i.InvitationStatus==InvitationStatus.Accepted).CountAsync();
            double percentAccepted;
            if (totalPeopleInvited == 0)
            {
                percentAccepted = 0;
            }
            else {
                percentAccepted = peopleAccepted / totalPeopleInvited * 100;
            }

            StatisticsViewModel viewModel = new()
            {
                EventsCreated = eventsCreated,
                PeopleAcceptedPercent = percentAccepted,
                PeopleInvited = totalPeopleInvited
            };

            return View(viewModel);
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
