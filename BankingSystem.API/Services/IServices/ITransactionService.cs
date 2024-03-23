﻿using BankingSystem.API.DTO;
using BankingSystem.API.Models;

namespace BankingSystem.API.Services.IServices
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsOfAccountAsync(Guid accountId);
        Task<Transaction> DepositTransactionAsync(DepositTransactionDTO transactionDto, Guid accountId, Guid userId);
        Task<Transaction> WithdrawTransactionAsync(WithdrawTransactionDTO withdrawDto, Guid accountId, int atmIdAtmCardPin);
    }
}