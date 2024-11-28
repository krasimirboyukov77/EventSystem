using EventSystem.Data;
using EventSystem.Data.Models;
using EventSystem.Services.Interfaces;
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
        private readonly IInviteService _inviteService;

        public InviteController(
            ApplicationDbContext context
           ,IInviteService inviteService)
        {
            _context = context;
            _inviteService = inviteService;
        }
        public async Task<IActionResult> Index()
        {
           var viewModel = await _inviteService.GetInvites();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> InviteUser(string userId, string eventId)
        {
            var isUserInvited = await _inviteService.InviteUser(userId, eventId);

            if (!isUserInvited)
            {
                return BadRequest();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string inviteId)
        {
            var isSuccessful = await _inviteService.Accept(inviteId);

            if (!isSuccessful)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Decline(string inviteId)
        {
            var isSuccessful = await _inviteService.Accept(inviteId);

            if (!isSuccessful)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));

        }

    }
}
