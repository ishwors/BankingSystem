using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace BankingSystem.API.Data.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetUsersAsync();
        Task<Users?> GetUserAsync(Guid Id);
        Task<Users?> GetUserByEmailAsync(string email);
        Task<Users> AddUsers(Users users);
        Task<Users> UpdateUsersAsync(Users users);
        Task<Users> UpdatePasswordAsync(Users users);
        void DeleteUser(Guid Id);
        Task<Users> PatchUserDetails(Guid Id, JsonPatchDocument<UserCreationDTO> userDetails);
    }
}
