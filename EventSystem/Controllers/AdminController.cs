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
                        EventName = eventName,  // Event name from the Event table
                        UserEmail = userEmail   // User email from the User table
                    });
                }
            }
            return View(eventDetails);

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