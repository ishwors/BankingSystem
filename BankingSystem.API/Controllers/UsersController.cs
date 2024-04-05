using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities.CustomAuthorizations;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService UserService)
        {
            userService = UserService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A list of <see cref="Users"/>.</returns>
        [HttpGet]
        [CustomAuthorize("TellerPerson")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            var users = await userService.GetUsersAsync();
            if (users == null)
            {
                var list = new List<Users>();
                return list;
            }
            return Ok(users);
        }

        /// <summary>
        /// Gets a user by Id.
        /// </summary>
        /// <param name="id">The Id of the user.</param>
        /// <returns>The user with the given Id.</returns>
        [HttpGet("{id}")]
        [RequireLoggedIn]
        public async Task<ActionResult<Users>> GetUser(Guid id)
        {
            var user = await userService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Logs in a user using the given credentials.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The logged in user.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<UserInfoDisplayDTO>> Login([FromBody] UserLoginDTO userlogin)
        {
            var user = await userService.Login(userlogin.UserName, userlogin.Password);
            if (user == null)
            {
                // return NotFound("Email or Password is incorrect.");
                return StatusCode(400, "Email or Password is incorrect.");
            }
            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await userService.Logout();

                // Clear authentication cookies or tokens
                //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Reset user identity
                HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                // Log any errors and return an error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new user using the given UserCreationDTO.
        /// </summary>
        /// <param name="user">The new user to create.</param>
        /// <returns>The created user.</returns>
        /// <response code="200">The created user.</response>
        /// <response code="400">If the user already exists.</response>
        [HttpPost]
        [CustomAuthorize("TellerPerson")]
        public async Task<ActionResult<Users>> AddUsers(UserCreationDTO user)
        {
            var users = await userService.RegisterUser(user);
            if (users == null)
            {
                return StatusCode(400, "User already exists.");
            }
            return Ok(users);
        }

        /// <summary>
        /// Deletes a user by Id.
        /// </summary>
        /// <param name="Id">The Id of the user to delete.</param>
        /// <returns>A NoContent response.</returns>
        [HttpDelete("{Id}")]
        [CustomAuthorize("TellerPerson")]
        public ActionResult DeleteUser(Guid Id)
        {
            userService.DeleteUser(Id);
            return NoContent();
        }

        /// <summary>
        /// Updates the user with the specified Id using the given UserUpdateDTO.
        /// </summary>
        /// <param name="Id">The Id of the user to update.</param>
        /// <param name="user">The updated user information.</param>
        /// <returns>The updated user.</returns>
        [HttpPut("{Id}")]
        [RequireLoggedIn]
        public async Task<ActionResult<Users>> UpdateUsers(Guid Id, UserUpdateDTO user)
        {
            var newUser = await userService.UpdateUsersAsync(Id, user);
            if (newUser == null)
            {
                return BadRequest("Update failed");
            }
            return Ok(newUser);
        }

        /// <summary>
        /// Resets the password of a user.
        /// </summary>
        /// <param name="username">The username of the user to reset the password for.</param>
        /// <param name="password">The new password to set.</param>
        /// <returns>The user with the updated password.</returns>
        [HttpPut("/resetPassword/{username}")]
        public async Task<ActionResult<Users>> ResetPassword(string username, string password)
        {
            var newUser = await userService.ResetUserPasswordAsync(username, password);
            if (newUser == null)
            {
                return BadRequest("Update failed");
            }
            return Ok(newUser);
        }

        /// <summary>
        /// Changes the password of a user.
        /// </summary>
        /// <param name="Id">The Id of the user to change password for.</param>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>The user with the updated password.</returns>
        [HttpPut("/changePassword/{Id}")]
        [RequireLoggedIn]
        public async Task<ActionResult<Users>> ChangePassword(Guid Id, string oldPassword, string newPassword)
        {
            var newUser = await userService.ChangePasswordAsync(Id, oldPassword, newPassword);
            if (newUser == null)
            {
                return BadRequest("Password Change failed");
            }
            return Ok(newUser);
        }

        /// <summary>
        /// Patches the user details by applying a JSON Patch Document to the user.
        /// </summary>
        /// <param name="Id">The Id of the user to be patched.</param>
        /// <param name="patchDocument">The JSON Patch Document to be applied to the user.</param>
        /// <returns>The patched user.</returns>
        [HttpPatch("{Id}")]
        [RequireLoggedIn]
        public async Task<ActionResult<Users>> PatchUserDetails(Guid Id, JsonPatchDocument<UserCreationDTO> patchDocument)
        {
            var user = await userService.PatchUserDetails(Id, patchDocument);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(user))
            {
                return BadRequest(ModelState);
            }
            if (user == null)
            {
                NotFound();
            }
            return Ok(user);
        }
    }
}
