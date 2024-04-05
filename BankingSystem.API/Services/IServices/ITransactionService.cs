using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;

namespace BankingSystem.API.Services.IServices
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDisplayDTO>> GetTransactionsOfAccountAsync(long accountNumber);
        Task<Transaction> DepositTransactionAsync(DepositTransactionDTO transactionDto, long accountNumber);
        Task<Transaction> WithdrawTransactionAsync(WithdrawTransactionDTO withdrawDto, long accountNumber, int atmIdAtmCardPin);
    }
}
