﻿using AutoMapper;
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
        private readonly AccountServices AccountServices;

        private readonly IMapper _mapper;

        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IPasswordHasher<Users> _passwordHasher;

        public UserService(IUserRepository userRepository, IMapper mapper, AccountServices accountServices, UserManager<Users> userManager, SignInManager<Users> signInManager, IPasswordHasher<Users> passwordHasher)
        {
            UserRepository = userRepository ?? throw new ArgumentOutOfRangeException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            AccountServices = accountServices;
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;
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

            var user = new Users()
            {
                UserName = finalUser.UserName,
                Fullname = finalUser.Fullname,
                Email = finalUser.Email,
                PhoneNumber = finalUser.PhoneNumber,
                Address = finalUser.Address,
                DateOfBirth = finalUser.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,

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

                    var accountNumber = RandomNumberGeneratorHelper.GenerateRandomNumber(1);
                    var atmCardNum = RandomNumberGeneratorHelper.GenerateRandomNumber(2);
                    var atmCardPin = (int)RandomNumberGeneratorHelper.GenerateRandomNumber(3);

                    var accountDTO = new AccountDTO(user.Id, accountNumber, 0, atmCardNum, atmCardPin, DateTime.UtcNow, user.Id, DateTime.UtcNow, user.Id);

                    var checkAccount = await AccountServices.GetAccountByUserIdAsync(user.Id);
                    if (checkAccount != null)
                    {
                        throw new Exception("User already has an account.");
                    }
                    await AccountServices.AddAccounts(accountDTO, users);
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
            finalUser.PasswordHash = users.Password;

            var existingUser = await GetUserAsync(Id);
            // Check if the existing user is null
            if (existingUser == null)
            {
                throw new Exception($"User with ID {Id} not found.");
            }
            //if password is not same as in the database; update it
            if (!string.IsNullOrEmpty(finalUser.PasswordHash) && _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, users.Password) != PasswordVerificationResult.Success)
            {
                // Hash the new password
                var newPasswordHash = _passwordHasher.HashPassword(existingUser, users.Password);
                finalUser.PasswordHash = newPasswordHash;
            }

            var user = await UserRepository.UpdateUsersAsync(Id, finalUser);
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
                    //return jwtToken;
                    var user = await AddRoleForDisplay(existingUser);// After generating the JWT token in your login method

                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(jwtToken);
                    foreach (var claim in token.Claims)
                    {
                        Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                    }
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
