﻿using BankingSystem.API.DTO;
using BankingSystem.API.Models;
using BankingSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionServices _transactionServices;

        public TransactionController(TransactionServices transactionServices)
        {
            _transactionServices = transactionServices ?? throw new ArgumentOutOfRangeException(nameof(transactionServices));
        }


        [Route("api/transactions")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(Guid accountId)
        {
            if (await _transactionServices.GetTransactionsOfAccountAsync(accountId) == null)
            {
                var list = new List<Transaction>();
                return list;
            }

            return Ok(await _transactionServices.GetTransactionsOfAccountAsync(accountId));
        }

        [Route("api/transactions/deposit")]
        [HttpPost]
        public async Task<ActionResult<Transaction>> DepositTransaction(DepositTransactionDTO transaction, Guid accountId, Guid tellerId)
        {
            var depositAccount = await _transactionServices.DepositTransactionAsync(transaction, accountId, tellerId);

            return Ok(depositAccount);
        }

        [Route("api/transactions/withdraw")]
        [HttpPost]
        public async Task<ActionResult<Transaction>> WithdrawTransaction(WithdrawTransactionDTO transaction, Guid accountId, int atmCardPin)
        {
            var withdrawAccount = await _transactionServices.WithdrawTransactionAsync(transaction, accountId, atmCardPin);

            return Ok(withdrawAccount);
        }
    }
}
