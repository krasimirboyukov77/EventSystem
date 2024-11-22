using EventSystem.Data;
using Microsoft.AspNetCore.Mvc;
using EventSystem.ViewModels.EventViewModel;
using System.Globalization;
using System.Security.Claims;
using EventSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EventSystem.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        //Това трябва да се замени със Service, само моментно решение
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Guid userId = GetUserId();

            List<UserEvent> userEvents = await _context.UsersEvents
                .Where(ue => ue.UserId == userId)
                .Include(ue => ue.Event)  
                .ToListAsync();

            List<Event> events = userEvents.Select(ue => ue.Event).ToList();

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
            };

            UserEvent newUserEvent = new()
            {
                EventId = newEvent.id,
                UserId = GetUserId()
            };

            await _context.Events.AddAsync(newEvent);
            await _context.UsersEvents.AddAsync(newUserEvent);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Event eventToEdit = await _context.Events.FindAsync(id);

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

            Event eventToUpdate = await _context.Events.FindAsync(viewModel.id);

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
            Event eventToDelete = await _context.Events.FindAsync(id);

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
            Event eventToDelete = await _context.Events.FindAsync(id);

            _context.Events.Remove(eventToDelete);
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
    }
}
