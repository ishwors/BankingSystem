using System.Security.Claims;

namespace BankingSystem.API.Utilities
{
    public class GetLoggedinUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetLoggedinUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentUserId()
        {
            Guid userId;
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out userId))
            {
                // currentUserId is successfully parsed as a GUID
                return userId;
            }
            return Guid.Empty;
        }
    }
}
