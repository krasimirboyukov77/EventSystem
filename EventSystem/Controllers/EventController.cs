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
            //В индекса може да изкараме всичките евенти и след това може да направим отделен метод, в който да се виждат тези създадени от usera
            //За да може да изглежда по-добре кода позлваме просто думата var. Тя взема просто типа на обекта, който се връща след операцията.
            //В случай като този, защото накрая казваме да е в лист и преди това в селекта го присвояваме с проекция в дадения виелмодел той сам си го слага да е в лист с дадения тип

            var entities = string.IsNullOrEmpty(searchTerm)
        ? await _context.Events.ToListAsync()
        : await _context.Events
        .Where(e =>
            e.Name.ToLower().Contains(searchTerm.ToLower()) ||
            e.Description.ToLower().Contains(searchTerm.ToLower()) ||
            e.Location.ToLower().Contains(searchTerm.ToLower()))
        .ToListAsync();

            var events = entities.Select(e => new DetailsEventViewModel()
            {
                id = e.id,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location
            });

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
               //В повечето случай като свети така зелено трябва да сложим една питанка на самия тип, за да го обозначим, че може да е нул стойност.
               //Тогава трябва да хванем ако стане нъл и да върнем някаква грешка или нешо друго, за да нямаме нъл стойности.
                //Винаги когато може да се върне нълл е задължително да го хванем и да върнем грешка. По-къснп ще направим наши страници за тях.
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


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            bool isEventGuidValid = Guid.TryParse(id, out var eventId);

            if (!isEventGuidValid) 
            { 
                return NotFound();
            }

            var eventDEtails = await _context.Events.FirstOrDefaultAsync(e => e.id == eventId);

            if (eventDEtails == null) 
            {
                return NotFound();
            }

            DetailsEventViewModel viewModel = new()
            {
                id = eventId,
                Name = eventDEtails.Name,
                Description = eventDEtails.Description,
                Date = eventDEtails.Date,
                Location = eventDEtails.Location,
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
        public IActionResult SearchPeople(string term)
        {
            // If the search term is empty or null, return an empty 

            // Example query to simulate fetching people by name from the database
            var users = _userManager.Users
         .Where(u => u.UserName.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
         .Select(u => new PersonInfo()
         {
             Id = u.Id,
             Name = u.UserName ?? string.Empty
         })
         .ToList();

            // Return the list of people as JSON
            return PartialView("_Peoplelist", users);
        }
    }
}
