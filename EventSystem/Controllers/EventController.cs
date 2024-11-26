using EventSystem.Data;
using Microsoft.AspNetCore.Mvc;
using EventSystem.ViewModels.EventViewModel;
using System.Globalization;
using System.Security.Claims;
using EventSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EventSystem.Data.Enum;
using System.Runtime.InteropServices;

namespace EventSystem.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        //Това трябва да се замени със Service, само моментно решение
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(ApplicationDbContext context
              ,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
           var entities = string.IsNullOrEmpty(searchTerm)
        ? await _context.Events.Include(e => e.Host).ToListAsync()
        : await _context.Events
        .Include(e=> e.Host)
        .Where(e =>
            e.Name.ToLower().Contains(searchTerm.ToLower()) ||
            e.Description.ToLower().Contains(searchTerm.ToLower()) ||
            e.Location.ToLower().Contains(searchTerm.ToLower()))
        .ToListAsync();

            var events = entities.Select(e => new DetailsEventViewModel()
            {
                id = e.id,
                Name = e.Name,
                HostName = e.Host.UserName ?? string.Empty,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location
            }).OrderBy(e => e.Date);//order by date added

            return View(events);
        }



        [HttpGet]
        public IActionResult Create()
        {
            CreateEventViewModel viewModel = new CreateEventViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            bool isDateValid = DateTime.TryParseExact(viewModel.Date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eventDate);
            if (!isDateValid)
            {
                return View(viewModel);
            }

            Event newEvent = new Event()
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Date = eventDate,
                Location = viewModel.Location,
                HostId = GetUserId()
            };

            UserEvent newUserEvent = new()
            {
                EventId = newEvent.id,
                UserId = GetUserId(),
                AttendStatus = AttendStatus.Host
            };

            await _context.Events.AddAsync(newEvent);
            await _context.UsersEvents.AddAsync(newUserEvent);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Event? eventToEdit = await _context.Events.FindAsync(id);

            if (eventToEdit == null)
            {
                return NotFound();
            }


            EditEventViewModel viewModel = new EditEventViewModel()
            {
                id = eventToEdit.id,
                Name = eventToEdit.Name,
                Description = eventToEdit.Description,
                Date = eventToEdit.Date.ToString("yyyy-MM-ddTHH:mm"),
                Location = eventToEdit.Location
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            Event? eventToUpdate = await _context.Events.FindAsync(viewModel.id);

            if (eventToUpdate == null)
            {
                return NotFound();
            }

            bool isDateValid = DateTime.TryParseExact(viewModel.Date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eventDate);
            if (!isDateValid)
            {
                return View(viewModel);
            }

            eventToUpdate.Name = viewModel.Name;
            eventToUpdate.Description = viewModel.Description;
            eventToUpdate.Date = eventDate;
            eventToUpdate.Location = viewModel.Location;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            Event? eventToDelete = await _context.Events.FindAsync(id);

            if(eventToDelete == null)
            {
                return NotFound();
            }

            DeleteEventViewModel viewModel = new DeleteEventViewModel
            {
                Id = eventToDelete.id,
                Name = eventToDelete.Name,
                Description = eventToDelete.Description,
                Date = eventToDelete.Date,
                Location = eventToDelete.Location
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var eventToDelete = await _context.Events
                .Include(e => e.UsersEvents) 
                .FirstOrDefaultAsync(e => e.id == id && !e.IsDeleted);

            if (eventToDelete == null)
            {
                return NotFound();
            }

            eventToDelete.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            bool isEventGuidValid = Guid.TryParse(id, out var eventId);

            if (!isEventGuidValid) 
            { 
                return NotFound();
            }

            var eventDetails = await _context.Events.Include(e=> e.Host).FirstOrDefaultAsync(e => e.id == eventId);

            if (eventDetails == null) 
            {
                return NotFound();
            }

            DetailsEventViewModel viewModel = new()
            {
                id = eventId,
                Name = eventDetails.Name,
                Description = eventDetails.Description,
                Date = eventDetails.Date,
                Location = eventDetails.Location,
                HostName = eventDetails.Host.UserName ?? string.Empty
            };
            var userId = GetUserId();

            viewModel.PopleAttending = await _context.UsersEvents
                .Include(e=> e.User)
                .Where(e => e.UserId == userId && e.EventId == viewModel.id)
                .Select(e=> new PersonInfo()
                {
                    Id = e.UserId,
                    Name = e.User.UserName ?? string.Empty,
                    AttendStatus = (int)e.AttendStatus
                }).ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPeople(string term, string eventId)
        {
            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return NotFound();
            }

            var usersGuidInEvent = await _context.UsersEvents.Where(ue=> ue.EventId == eventGuid).Select(ue=> ue.UserId).ToListAsync();
            var users = _userManager.Users
         .Where(u => u.Id != GetUserId() && (u.UserName.ToLower().Contains(term) || u.Email.ToLower().Contains(term)) && usersGuidInEvent.Contains(u.Id) == false)
         .Select(u => new PersonInfo()
         {
             Id = u.Id,
             Name = u.UserName ?? string.Empty
         })
         .ToList();


            return PartialView("_Peoplelist", users);
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonToEvent(string personId, string eventId)
        {

            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return NotFound();
            }

            bool isUserGuidValid = Guid.TryParse(personId, out var userGuid);

            if (!isUserGuidValid)
            {
                return NotFound();
            }
            var isEventValid = await _context.Events.FirstOrDefaultAsync(e => e.id == eventGuid);

            if (isEventValid == null)
            {
                return Json(new { success = false, message = "Event not found." });
            }

            var person = _userManager.Users.FirstOrDefault(u => u.Id == userGuid);
            if (person == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            UserEvent userEventToAdd = new()
            {
                UserId = userGuid,
                EventId = eventGuid,
                AttendStatus = AttendStatus.Invited
            };
            
            await _context.UsersEvents.AddAsync(userEventToAdd);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Person added to the event." });
        }
    }
}
