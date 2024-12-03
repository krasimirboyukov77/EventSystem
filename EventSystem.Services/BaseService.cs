
using EventSystem.Data.Models;
using EventSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EventSystem.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public BaseService(
            IHttpContextAccessor httpContextAccessor,
             UserManager<ApplicationUser> userManager)
        {
           _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public Guid GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Guid.Empty;
            }
            if (Guid.TryParse(userId, out Guid userGuid))
            {

                return userGuid;
            }

            return Guid.Empty;
        }

        public async Task<bool> IsUserAdmin()
        {
            var user = await _userManager.FindByIdAsync(GetUserId().ToString());

            if (user == null)
            {
                return false;
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin == false)
            {
                return false;
            }

            return true;
        }
    }
}
