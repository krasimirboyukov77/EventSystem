
using EventSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EventSystem.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseService(IHttpContextAccessor httpContextAccessor)
        {
           _httpContextAccessor = httpContextAccessor;
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
    }
}
