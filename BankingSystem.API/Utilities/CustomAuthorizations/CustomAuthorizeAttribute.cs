using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BankingSystem.API.Utilities.CustomAuthorizations
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public CustomAuthorizeAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // If the user is not authenticated, return 401 Unauthorized
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if the user is in the required role
            if (!string.IsNullOrEmpty(_role) && !context.HttpContext.User.IsInRole(_role))
            {
                // If the user is not in the required role, return 403 Forbidden
                context.Result = new StatusCodeResult(403);
                return;
            }
        }
    }
}
