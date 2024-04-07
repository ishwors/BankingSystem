using BankingSystem.API.Entities;

namespace BankingSystem.API.Data.Repository.IRepository
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Accounts>> GetAccountsAsync();
        Task<Accounts?> GetAccountAsync(Guid accountId);
        Task<IEnumerable<Accounts>> GetAccountsByIdsAsync(List<Guid> accountIds);
        Task<Accounts?> GetAccountByAccountNumberAsync(long accountNumber);
        Task<Accounts?> GetAccountByUserIdAsync(Guid userId);
        Task<Accounts> AddAccounts(Accounts accounts);
        Task<Accounts> UpdateAccountsAsync(Guid accountId, Accounts accounts);
        void DeleteAccount(Guid accountId);
        // Task<Accounts> PatchAccountDetails(Guid accountId, JsonPatchDocument<AccountDTO> aDetails);
        Task<Accounts> UpdateAccountAsync(Guid accountId, object finalAccount);
    }
}
