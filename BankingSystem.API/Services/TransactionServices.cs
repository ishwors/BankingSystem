using AutoMapper;
using BankingSystem.API.Data.Repository;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities;
using BankingSystem.API.Utilities.EmailTemplates;
using System.Security.Principal;

namespace BankingSystem.API.Services
{
    public class TransactionServices : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly GetLoggedinUser _getLoggedinUser;

        public TransactionServices(ITransactionRepository transactionRepository, IMapper mapper, IEmailService emailService, IUserService userService, IAccountService accountService, GetLoggedinUser getLoggedinUser)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentOutOfRangeException(nameof(transactionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _accountService = accountService ?? throw new ArgumentException(nameof(accountService));
            _getLoggedinUser = getLoggedinUser;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsOfAccountAsync(long accountNumber)
        {
            return await _transactionRepository.GetTransactionsOfAccountAsync(accountNumber);
        }

        public void DeleteTransaction(Guid accountId, Guid transactionId)
        {
            _transactionRepository.DeleteTransaction(accountId, transactionId);
        }

        public async Task<bool> TransactionExistAsync(Guid transactionId)
        {
            return await _transactionRepository.TransactionExistAsync(transactionId);
        }

        public async Task<bool> IsVerifiedKycAsync(Guid kycId)
        {
            return await _transactionRepository.IsVerifiedKycAsync(kycId);
        }

        public async Task<Transaction> DepositTransactionAsync(DepositTransactionDTO transactionDto, long accountNumber, Guid userId)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);

            var depositTeller = _getLoggedinUser.GetCurrentUserId();
            transaction.LoggedInTeller = depositTeller;

            var depositedTransaction = await _transactionRepository.DepositTransactionAsync(transaction, accountNumber, userId);

            if (depositedTransaction != null)
            {

                //get the account object from accountNumber
                var account = await _accountService.GetAccountByAccountNumberAsync(accountNumber);

                //get the user object from userId in account object
                var user = await _userService.GetUserAsync(account.UserId);

                //get the email body string from Email Templates file
                var emailBody = EmailTemplates.EmailBodyForTransactionDeposit(user.Fullname, transactionDto.Amount, transactionDto.TransactionRemarks, transaction.TransactionTime);

                // Prepare email
                var email = new Email
                {
                    MailSubject = "Amount Deposited",
                    MailBody = emailBody,
                    ReceiverEmail = user.Email // Use the user's email address obtained from the UserDTO
                };

                // Send email
                await _emailService.SendEmailAsync(email);
            }

            return depositedTransaction;
        }

        public async Task<Transaction> WithdrawTransactionAsync(WithdrawTransactionDTO withdrawDto, long accountNumber, int atmIdAtmCardPin)
        {
            var transaction = _mapper.Map<Transaction>(withdrawDto);

            var withdrawnTransaction = await _transactionRepository.WithdrawTransactionAsync(transaction, accountNumber, atmIdAtmCardPin);

            if (withdrawnTransaction != null)
            {

                //get the account object from accountNumber
                var account = await _accountService.GetAccountByAccountNumberAsync(accountNumber);

                //get the user object from userId in account object
                var user = await _userService.GetUserAsync(account.UserId);

                //get the email body string from Email Templates file
                var emailBody = EmailTemplates.EmailBodyForTransactionDeposit(user.Fullname, withdrawDto.Amount, withdrawDto.TransactionRemarks, transaction.TransactionTime);

                // Prepare email
                var email = new Email
                {
                    MailSubject = "Amount Withdrawn",
                    MailBody = emailBody,
                    ReceiverEmail = user.Email // Use the user's email address obtained from the UserDTO
                };

                // Send email
                await _emailService.SendEmailAsync(email);

            }
            return withdrawnTransaction;
        }
    }
}
