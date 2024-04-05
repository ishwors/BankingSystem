using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities.CustomAuthorizations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{
    /// <summary>
    /// Controller for handling transactions
    /// </summary>
    [ApiController]
    [Route("/api/transactions")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionServices;
        private readonly UserManager<Users> _userManager;


        /// <summary>
        /// Constructor for TransactionController
        /// </summary>
        /// <param name="transactionServices">Instance of TransactionServices</param>
        /// <param name="userManager">Instance of UserManager</param>
        public TransactionController(ITransactionService transactionServices, UserManager<Users> userManager)
        {
            _transactionServices = transactionServices ?? throw new ArgumentOutOfRangeException(nameof(transactionServices));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// Gets all transactions associated with an account
        /// </summary>
        /// <param name="accountId">Id of the account</param>
        /// <returns>List of transactions</returns>
        /// <response code="200">Returns the transactions for the given account</response>
        /// <response code="404">If no transactions are found</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransactionDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{accountNumber}")]
        [RequireLoggedIn]
        public async Task<ActionResult<IEnumerable<TransactionDisplayDTO>>> GetTransactions(long accountNumber)
        {
            var transactions = await _transactionServices.GetTransactionsOfAccountAsync(accountNumber);
            if (transactions == null)
            {
                var list = new List<TransactionDisplayDTO>();
                return NotFound(list);
            }
            return Ok(transactions);
        }

        /// <summary>
        /// Deposits funds into an account
        /// </summary>
        /// <param name="transaction">Deposit transaction details</param>
        /// <param name="accountNumber">Account number to be credited</param>
        /// <returns>The deposit transaction</returns>
        /// <response code="200">Returns the deposit transaction details</response>
        [HttpPost]
        [CustomAuthorize("TellerPerson")]
        [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
        [Route("deposit")]
        public async Task<ActionResult<Transaction>> DepositTransaction(DepositTransactionDTO transaction, long accountNumber)
        {          
            var depositAccount = await _transactionServices.DepositTransactionAsync(transaction, accountNumber);
            
            return Ok(depositAccount);
        }

        /// <summary>
        /// Withdraws funds from an account
        /// </summary>
        /// <param name="transaction">Withdraw transaction details</param>
        /// <param name="accountNumber">Account number to be debited</param>
        /// <param name="atmCardPin">ATM card PIN for authentication</param>
        /// <returns>The withdraw transaction</returns>
        /// <response code="200">Returns the withdraw transaction details</response>
        [HttpPost]
        [CustomAuthorize("AccountHolder")]
        [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
        [Route("withdraw")]
        public async Task<ActionResult<Transaction>> WithdrawTransaction(WithdrawTransactionDTO transaction, long accountNumber, int atmCardPin)
        {
            var withdrawAccount = await _transactionServices.WithdrawTransactionAsync(transaction, accountNumber, atmCardPin);

            return Ok(withdrawAccount);
        }
    }
}
