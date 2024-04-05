using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace BankingSystem.API.Services.IServices
{
    public interface IUserService
    {
        Task<Users?> GetUserAsync(Guid Id);
        Task<Users?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserInfoDisplayDTO>> GetUsersAsync();
        Task<UserInfoDisplayDTO> RegisterUser(UserCreationDTO users);
        void DeleteUser(Guid Id);
        Task<UserInfoDisplayDTO> PatchUserDetails(Guid Id, JsonPatchDocument<UserCreationDTO> patchDocument);
        Task<UserInfoDisplayDTO> UpdateUsersAsync(Guid Id, UserUpdateDTO users);
        Task<UserInfoDisplayDTO> ResetUserPasswordAsync(string username, string password); 
        Task<UserInfoDisplayDTO> ChangePasswordAsync(Guid Id, string oldPassword, string newPassword);
        Task<UserInfoDisplayDTO> Login(string username, string password);
        Task Logout();
    }
}
