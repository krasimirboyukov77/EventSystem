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
            var events = _context.Events.ToListAsync();

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

            return RedirectToAction("Index", "Home");
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
