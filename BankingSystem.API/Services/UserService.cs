using AutoMapper;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingSystem.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository UserRepository;
        private readonly IAccountService AccountServices;

        private readonly IMapper _mapper;

        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IPasswordHasher<Users> _passwordHasher;

       private readonly GetLoggedinUser _getLoggedinUser;

        public UserService(IUserRepository userRepository, IMapper mapper, IAccountService accountServices, UserManager<Users> userManager, SignInManager<Users> signInManager, IPasswordHasher<Users> passwordHasher, GetLoggedinUser getLoggedinUser)
        {
            UserRepository = userRepository ?? throw new ArgumentOutOfRangeException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            AccountServices = accountServices;
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;
            //_httpContextAccessor = httpContextAccessor;
            _getLoggedinUser = getLoggedinUser;
        }

        public async Task<Users?> GetUserAsync(Guid Id)
        {
            //returns only user detail
            return await UserRepository.GetUserAsync(Id);
        }
        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            //returns only user detail
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<UserInfoDisplayDTO>> GetUsersAsync()
        {
            var users = _userManager.Users.AsQueryable().ToList();
            var userDTOs = new List<UserInfoDisplayDTO>();

            foreach (var user in users)
            {
                var userDTO = await AddRoleForDisplay(user);
                userDTOs.Add(userDTO);
            }
            return userDTOs;
        }

        public async Task<UserInfoDisplayDTO> RegisterUser(UserCreationDTO users)
        {
            var finalUser = _mapper.Map<Users>(users);
            finalUser.PasswordHash = users.Password;

            checkValidation(users);

            var userId = _getLoggedinUser.GetCurrentUserId();
            finalUser.CreatedBy = userId;

            var user = new Users()
            {
                UserName = finalUser.UserName,
                Fullname = finalUser.Fullname,
                Email = finalUser.Email,
                PhoneNumber = finalUser.PhoneNumber,
                Address = finalUser.Address,
                DateOfBirth = finalUser.DateOfBirth,
                CreatedBy= finalUser.CreatedBy,
                CreatedAt = DateTime.UtcNow,

                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
            };
            try
            {
                var result = await _userManager.CreateAsync(user, finalUser.PasswordHash);
                if (result.Errors.Any())
                {
                    var description = result.Errors.Select(x => x.Description).First();
                    throw new Exception(description);
                }

                if (users.UserType == UserRoles.TellerPerson)
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.TellerPerson.ToString());
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.AccountHolder.ToString());
                    await CreateUserAccount(user, userId);
                }
                return await AddRoleForDisplay(user);
            }
            catch (Exception e)
            {
                var errorMsg = $"Could not create user!, {e}";
                Console.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }
        }

        private void checkValidation(UserCreationDTO users)
        {
            var emailDuplication = _userManager.FindByEmailAsync(users.Email);
            if (emailDuplication.Result != null)
            {
                throw new Exception("Duplicate Email Address!");
            }

            var usernameDuplication = _userManager.FindByNameAsync(users.UserName);
            if (usernameDuplication.Result != null)
            {
                throw new Exception("Duplicate UserName!");
            }
        }

        private async Task CreateUserAccount(Users user, Guid userId)
        {
            long accountNumber;
            bool accountExists;

            do
            {
                //to generate random account number
                accountNumber = RandomNumberGeneratorHelper.GenerateRandomNumber(1);
                accountExists = await AccountServices.GetAccountByAccountNumberAsync(accountNumber) != null;
            } while (accountExists);

            var accountDTO = new Accounts
            {
                AccountId = Guid.NewGuid(),
                UserId = user.Id,
                AccountNumber = accountNumber,
                //to generate random atm card number
                AtmCardNum = RandomNumberGeneratorHelper.GenerateRandomNumber(2),
                //to generate random atm card pin
                AtmCardPin = (int)RandomNumberGeneratorHelper.GenerateRandomNumber(3),
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ModifiedAt = null,
                ModifiedBy = null,
            };

            var checkAccount = await AccountServices.GetAccountByUserIdAsync(user.Id);
            if (checkAccount != null)
            {
                throw new Exception("User already has an account.");
            }
            await AccountServices.AddAccounts(accountDTO, user.Email);
        }

        public void DeleteUser(Guid Id)
        {
            UserRepository.DeleteUser(Id);
        }

        public async Task<UserInfoDisplayDTO> PatchUserDetails(Guid Id, JsonPatchDocument<UserCreationDTO> patchDocument)
        {
            var user = await UserRepository.PatchUserDetails(Id, patchDocument);
            return await AddRoleForDisplay(user);
        }

        public async Task<UserInfoDisplayDTO> UpdateUsersAsync(Guid Id, UserUpdateDTO users)
        {
            var finalUser = _mapper.Map<Users>(users);

            var existingUser = await GetUserAsync(Id);
            ValidateExistingUser(existingUser, Id);

            //user exists: add Id to the incoming changes for the user
            finalUser.Id = Id;

            var userId = _getLoggedinUser.GetCurrentUserId();
            existingUser.ModifiedBy = userId;

            var user = await UserRepository.UpdateUsersAsync(finalUser);
            return await AddRoleForDisplay(user);
        }

        public async Task<UserInfoDisplayDTO> ResetUserPasswordAsync(string userName, string password)
        {
            var existingUser = await _userManager.FindByNameAsync(userName);
            // Check if the existing user is null
            if (existingUser == null)
            {
                throw new Exception($"User with username {userName} not found.");
            }

            //if password is not empty and not same as in the database; update it
            if (!string.IsNullOrEmpty(password) && _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, password) != PasswordVerificationResult.Success)
            {
                // Hash the new password
                var newPasswordHash = _passwordHasher.HashPassword(existingUser, password);
                existingUser.PasswordHash = newPasswordHash;
            }
            var userId= _getLoggedinUser.GetCurrentUserId(); 
            existingUser.ModifiedBy = userId;
            
            var user = await UserRepository.UpdatePasswordAsync(existingUser);
            return await AddRoleForDisplay(user);
        }

        private void ValidateExistingUser(Users? existingUser, Guid Id)
        {
            // Check if the existing user is null
            if (existingUser == null)
            {
                throw new Exception($"User with ID {Id} not found.");
            }
        }

        public async Task<UserInfoDisplayDTO> ChangePasswordAsync(Guid Id, string oldPassword, string newPassword)
        {
            var existingUser = await GetUserAsync(Id);
            ValidateExistingUser(existingUser, Id);

            //if password is not empty and oldPassword == database password
            if (!string.IsNullOrEmpty(oldPassword) && !string.IsNullOrEmpty(newPassword) && _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, oldPassword) == PasswordVerificationResult.Success)
            {
                //if password is not empty and not same as in the database; update it
                if (_passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, newPassword) != PasswordVerificationResult.Success){
                    // Hash the new password
                    var newPasswordHash = _passwordHasher.HashPassword(existingUser, newPassword);
                    existingUser.PasswordHash = newPasswordHash;
                }
                else
                {
                    throw new Exception($"New Password cannot be same as the old one.");
                }
            }
            else
            {
                throw new Exception($"Old Password is incorrect.");
            }

            var userId = _getLoggedinUser.GetCurrentUserId();
            existingUser.ModifiedBy = userId;
           
            var user = await UserRepository.UpdatePasswordAsync(existingUser);
            return await AddRoleForDisplay(user);
        }

        public async Task<UserInfoDisplayDTO> Login(string username, string password)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(username, password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // User is successfully logged in, retrieve the user from the database
                    var existingUser = await _userManager.FindByNameAsync(username);
                    var jwtToken = await GenerateJwtToken(existingUser); // Generate JWT token
                    var user = await AddRoleForDisplay(existingUser);// After generating the JWT token in your login method
                    return user;
                }
                else
                {
                    // Handle failed login attempt
                    throw new Exception("Invalid login attempt.");
                }
            }
            catch (Exception e)
            {
                // Log and re-throw the exception
                Console.WriteLine($"Error occurred during user login: {e}");
                throw;
            }
        }

        private async Task<string> GenerateJwtToken(Users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bootcamp-aloi-net-deploy-aws-secret-key"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var userType = roles.FirstOrDefault(); // Assuming a user can have only one role

            var claims = new[]
            {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Convert user.Id to string
               new Claim(ClaimTypes.Name, user.UserName),
               new Claim(ClaimTypes.Role, userType)
            // Add additional claims as needed (e.g., roles)
        };

            var token = new JwtSecurityToken(
                issuer: "your-issuer",
                audience: "your-audience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token expiration time
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public async Task<UserInfoDisplayDTO> AddRoleForDisplay(Users user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userType = roles.FirstOrDefault(); // Assuming a user can have only one role
            var userDTO = _mapper.Map<UserInfoDisplayDTO>(user);
            userDTO.UserType = userType;
            return userDTO;
        }

    }
}
