﻿using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services;
using BankingSystem.API.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionServices _transactionServices;
        private readonly UserManager<Users> _userManager;

        public TransactionController(TransactionServices transactionServices, UserManager<Users> userManager)
        {
            _transactionServices = transactionServices ?? throw new ArgumentOutOfRangeException(nameof(transactionServices));
            _userManager= userManager ?? throw new ArgumentNullException(nameof(userManager));
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
        [CustomAuthorize("TellerPerson")]
        public async Task<ActionResult<Transaction>> DepositTransaction(DepositTransactionDTO transaction, long accountNumber)
        {
            // Get the user associated with the current HttpContext.User
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var depositAccount = await _transactionServices.DepositTransactionAsync(transaction, accountNumber, user.Id);

            return Ok(depositAccount);
        }

        [Route("api/transactions/withdraw")]
        [HttpPost]
        [CustomAuthorize("AccountHolder")]
        public async Task<ActionResult<Transaction>> WithdrawTransaction(WithdrawTransactionDTO transaction, long accountNumber, int atmCardPin)
        {
            var withdrawAccount = await _transactionServices.WithdrawTransactionAsync(transaction, accountNumber, atmCardPin);

            return Ok(withdrawAccount);
        }
    }
}
