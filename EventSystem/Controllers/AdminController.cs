using EventSystem.Data.Models;
using EventSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EventSystem.ViewModels.EventViewModel;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Web.Helpers;
using EventSystem.ViewModels.AdminViewModel;

namespace EventSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminController(ApplicationDbContext context,UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userEvents = await _context.UsersEvents.ToListAsync();

            
            var eventDetails = new List<UserEventsViewModel>();

            foreach (var userEvent in userEvents)
            {
                var eventId = await _context.Events.FirstOrDefaultAsync(e => e.id == userEvent.EventId);
                    
                
                var eventName = await _context.Events
                    .Where(e => e.id == userEvent.EventId)
                    .Select(e => e.Name)
                    .FirstOrDefaultAsync();

                
                var userEmail = await _userManager.Users
                    .Where(u => u.Id == userEvent.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(userEmail))
                {
                    eventDetails.Add(new UserEventsViewModel
                    {
                        EventId = eventId.id,
                        EventName = eventName,  
                        UserEmail = userEmail   
                    });
                }
            }

            var registeredUsers = await _userManager.Users
                .Select(u => new RegisteredUsersViewModel
                {
                    UserId = u.Id,
                    UserEmail = u.Email,
                    UserName = u.UserName 
                })
                .ToListAsync();

            var viewModel = new CombinedUserEventViewModel
            {
                EventDetails = eventDetails,
                RegisteredUsers = registeredUsers
            };
            return View(viewModel);

        }
        public async Task<IActionResult> Details(Guid eventId)
        {
            if (eventId == Guid.Empty)
            {
                return NotFound("Event not found.");
            }

            var eventDetails = await _context.Events
                .FirstOrDefaultAsync(e => e.id == eventId);

            if (eventDetails == null)
            {
                return NotFound("Event not found.");
            }

            var userEvent = await _context.UsersEvents
                .FirstOrDefaultAsync(ue => ue.EventId == eventId);

            if (userEvent == null)
            {
                return NotFound("User for this event not found.");
            }

            var user = await _userManager.FindByIdAsync(userEvent.UserId.ToString());

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var eventViewModel = new DetailsAdminViewModel
            {
                Eventid = eventDetails.id,
                EventName = eventDetails.Name,
                EventDescription = eventDetails.Description,
                EventDate  = eventDetails.Date,
                EventLocation = eventDetails.Location,
                Userid = user.Id,
                UserEmail = user.Email
            };
            eventViewModel.PopleAttending = await _context.UsersEvents
                .Include(e => e.User)
                .Where(e => e.UserId == user.Id && e.EventId == eventViewModel.Eventid)
                .Select(e => new PersonInfo()
                {
                    Id = e.UserId,
                    Name = e.User.UserName ?? string.Empty,
                    AttendStatus = (int)e.AttendStatus
                }).ToListAsync();

            return View(eventViewModel);
        }

        //Добавих го за да можем от някъде да слагаме ролята, по-нататък може на друго място да го сложим
        //да не е в navbara.
        [HttpGet]
        public IActionResult AssignAdminRole()
        {
            AssignAdminViewModel model = new AssignAdminViewModel();
            return View(model);
        }
        

        [HttpPost]
        public async Task<IActionResult> AssignAdminRole(AssignAdminViewModel model)
        {
            var roleExist = await _roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                var role = new IdentityRole<Guid>("Admin");
                await _roleManager.CreateAsync(role);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                ModelState.AddModelError(string.Empty, "This user is already an admin.");
                return View(model);
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");

            if (result.Succeeded)
            {
                Admin admin = new Admin()
                {
                    UserId = user.Id,
                    SpecialPermissions = "Full Access",
                    User = user
                };

                _context.Add(admin);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Admin role assigned successfully!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Failed to assign Admin role.");
            return View(model);

        }

    }

}