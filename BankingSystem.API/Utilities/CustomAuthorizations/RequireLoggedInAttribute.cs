using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BankingSystem.API.Utilities.CustomAuthorizations
{
    public class RequireLoggedInAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // If the user is not authenticated, return 401 Unauthorized
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
