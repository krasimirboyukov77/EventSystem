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
using Microsoft.Extensions.Logging;
using EventSystem.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using EventSystem.Services;

namespace EventSystem.Controllers
{
    [Authorize]
    public class EventController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(ApplicationDbContext context
              ,UserManager<ApplicationUser> userManager
              ,IEventService eventService)
        {
            _context = context;
            _userManager = userManager;
            _eventService = eventService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? location, DateTime? date)
        {
            //var events = await _eventService.GetEventsByDate(searchTerm);
            var events = await _eventService.GetEventsByFilters(searchTerm, location, date);

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

            var isCreationSuccessfull = await _eventService.CreateEvent(viewModel);

            if (!isCreationSuccessfull)
            {
                ViewData["ErrorMessage"] = "Invalid data! Check inputs!";
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
          var viewModel = await _eventService.GetEventForEdit(id);

            if(viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

           var isUpdateSuccessfull = await _eventService.EditEvent(viewModel);

            if (!isUpdateSuccessfull)
            {
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
           var eventToDelete = await _eventService.GetEventForDelete(id);

            if(eventToDelete == null)
            {
                return NotFound();
            }

            return View(eventToDelete);
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

        [HttpPost]
        public async Task<IActionResult> Attend(string eventId)
        {
            var isAttendSuccessful = await _eventService.Attend(eventId);

            if (!isAttendSuccessful)
            {
                return BadRequest();
            }

            ViewData["SuccessMessage"] = "Successfully registered for the event!";
            return RedirectToAction("Details", new { id = eventId });
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var eventDetails = await _eventService.GetEventDetails(id);

            if (eventDetails == null)
            {
                return NotFound();
            }

            return View(eventDetails);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPeople(string term, string eventId)
        {
            var users = await _eventService.GetUsersInfo(term, eventId);

            if(users == null)
            {
                return NotFound();
            }

            return PartialView("_Peoplelist", users);
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonToEvent(string personId, string eventId)
        {

          var isPersonAdded = await _eventService.AddPersonToEvent(personId, eventId);

            if (!isPersonAdded)
            {
                Json(new { success = false, message = "Error occured!" });
            }

            return Json(new { success = true, message = "Person added to the event." });
        }

        [HttpGet]
        public async Task<IActionResult> Users(string id)
        {
            var users = await _eventService.GetUsersInEvent(id);

            if( users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(string userId, string eventId)
        {
            var isSuccessful = await _eventService.RemoveUser(userId, eventId);

            if (!isSuccessful)
            {
                return NotFound();
            }

            return Json(new { success = true });
        }
    }
}
