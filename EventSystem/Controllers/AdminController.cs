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
using System.Globalization;

namespace EventSystem.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userEvents = await _context.UsersEvents.ToListAsync();

            var recentlyDeletedEvents =  _context.Events
                .IgnoreQueryFilters()
                .Where(e=> e.IsDeleted == true)
                .Select(e => new RecentlyDeletedEventsViewModel
                {
                    EventId = e.id,
                    EventName = e.Name,
                    EventDescription = e.Description,
                    EventLocation = e.LocationName
                })
                .ToList();

            var eventDetails = new List<UserEventsViewModel>();

            foreach (var userEvent in userEvents)
            {
                var eventInfo = await _context.Events.FirstOrDefaultAsync(e => e.id == userEvent.EventId);

                if(eventInfo == null)
                {
                    return NotFound();
                }

                var eventId = eventInfo.id;


                var eventName = eventInfo.Name;

                
                var userEmail = await _userManager.Users
                    .Where(u => u.Id == userEvent.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(userEmail))
                {
                    eventDetails.Add(new UserEventsViewModel
                    {
                        EventId = eventId,
                        EventName = eventName,  
                        UserEmail = userEmail   
                    });
                }
            }

            var registeredUsers = await _userManager.Users
                .Select(u => new RegisteredUsersViewModel
                {
                    UserId = u.Id,
                    UserEmail = u.Email ?? string.Empty,
                    UserName = u.UserName ?? string.Empty
                })
                .ToListAsync();


            var viewModel = new CombinedUserEventViewModel
            {
                EventDetails = eventDetails,
                RegisteredUsers = registeredUsers,
                RecentlyDeletedEvents = recentlyDeletedEvents
            };
            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> DetailsEvent(Guid eventId)
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

            var eventViewModel = new DetailsEventAdminViewModel
            {
                Eventid = eventDetails.id,
                EventName = eventDetails.Name,
                EventDescription = eventDetails.Description,
                EventDate  = eventDetails.Date,
                EventLocation = eventDetails.LocationName,
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

        [HttpGet]
        public async Task<IActionResult> EditEvent(Guid id)
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
                Location = eventToEdit.LocationName
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditEvent(EditEventViewModel viewModel)
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
            eventToUpdate.LocationName = viewModel.Location;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            Event? eventToDelete = await _context.Events.FindAsync(id);

            if (eventToDelete == null)
            {
                return NotFound();
            }

            DeleteEventViewModel viewModel = new DeleteEventViewModel
            {
                Id = eventToDelete.id,
                Name = eventToDelete.Name,
                Description = eventToDelete.Description,
                Date = eventToDelete.Date,
                Location = eventToDelete.LocationName
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

        [HttpGet]
        public async Task<IActionResult> DetailsUsers(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return NotFound("User not found.");
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            var userDetailsViewModel = new RegisteredUsersViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email
            };
            return View(userDetailsViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound("User not found.");
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var editUserViewModel = new EditUserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                NewPassword = null
            };

            return View(editUserViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.UserName = model.UserName;
            user.Email = model.UserEmail;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (removePasswordResult.Succeeded)
                {
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        foreach (var error in addPasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            return RedirectToAction(nameof(DetailsUsers), new { userId = user.Id });
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