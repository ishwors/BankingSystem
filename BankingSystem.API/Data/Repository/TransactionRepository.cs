using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BankingSystem.API.Data.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private RoleManager<IdentityRole<Guid>> _roleManager;

        public TransactionRepository(ApplicationDbContext context, UserManager<Users> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context ?? throw new ArgumentOutOfRangeException(nameof(context));
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task<IEnumerable<Transaction>> GetTransactionsOfAccountAsync(long accountNumber)
        {
            var account = await _context.Account.Where(a => a.AccountNumber == accountNumber).FirstOrDefaultAsync();
            return await _context.Transactions
                .Where(p => p.AccountId == account.AccountId)
                .ToArrayAsync();
        }


        public async Task<Transaction?> GetTransactionFromAccountAsync(Guid accountId, Guid transactionId)
        {
            return await _context.Transactions
                .Where(p => p.AccountId == accountId && p.TransactionId == transactionId)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteTransaction(Guid accountId, Guid transactionId)
        {
            var delTransaction = await GetTransactionFromAccountAsync(accountId, transactionId);
            if (delTransaction != null)
            {
                _context.Transactions.Remove(delTransaction);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> TransactionExistAsync(Guid transactionId)
        {
            return await _context.Transactions.AnyAsync(c => c.TransactionId == transactionId);
        }


        public async Task<bool> IsVerifiedKycAsync(Guid kycId)
        {
            return await _context.KycDocuments.AnyAsync(c => c.KYCId == kycId);
        }

        public async Task<Transaction> DepositTransactionAsync(Transaction transaction, long accountNumber, Guid userId)
        {
            var account = await _context.Account
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (account is null)
            {
                throw new Exception($"Account with number {accountNumber} not found.");
            }

            var kycAccount = await _context.KycDocuments
                .FirstOrDefaultAsync(c => c.UserId == account.UserId);

            if (kycAccount is null)
            {
                throw new Exception($"KYC document not found for user ID {account.UserId}");
            }

            var teller = await _context.SystemUser
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (teller is null)
            {
                throw new Exception($"Teller id- {userId} is not found.");
            }

            // Check if the user has the TellerPerson role
            bool isTeller = await _userManager.IsInRoleAsync(teller, UserRoles.TellerPerson.ToString());

            if (!isTeller)
            {
                throw new Exception($"Teller id {userId} is not a valid/available.");
            }

            var isVerified = await IsVerifiedKycAsync(kycAccount.KYCId);

            if (isVerified is true )//&& isTeller)
            {
                // Set the accountId for the transaction
                transaction.AccountId = account.AccountId;

                _context.Transactions.Add(transaction);

                account.Balance += transaction.Amount;
                account.ModifiedBy = userId;
                account.ModifiedAt= DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return transaction;
            }
            else
            {
                throw new Exception("KYC is not verified, transaction cannot be made.");
            }
        }

        public async Task<Transaction> WithdrawTransactionAsync(Transaction transaction, long accountNumber, int atmIdAtmCardPin)
        {
            var account = await _context.Account
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (account is null)
            {
                throw new Exception($"Account with number {accountNumber} not found.");
            }

            var kycAccount = await _context.KycDocuments
                .FirstOrDefaultAsync(c => c.UserId == account.UserId);

            if (kycAccount is null)
            {
                throw new Exception($"KYC document not found for user Id {account.User.Id}.");
            }

            var atmPin = await _context.Account
                .FirstOrDefaultAsync(c => c.AtmCardPin == atmIdAtmCardPin);

            if (atmPin is null)
            {
                throw new Exception($"ATM Card Pin is not found or available for user ID {account.UserId}.");
            }

            var isVerified = await IsVerifiedKycAsync(kycAccount.KYCId);

            var withdrawAmount = transaction.Amount;

            if (account.Balance < transaction.Amount)
            {
                throw new Exception($"Insufficient balance for withdraw.");
            }

            if (isVerified is true)
            {
                transaction.AccountId = account.AccountId;

                _context.Transactions.Add(transaction);

                account.Balance -= transaction.Amount;
                account.ModifiedBy = account.UserId;
                account.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return transaction;
            }
            else
            {
                throw new Exception("KYC is not verified, transaction cannot be made.");
            }
        }
    }
}
