using AutoMapper;
using BankingSystem.API.Data.Repository;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities.EmailTemplates;
using System.Security.Principal;

namespace BankingSystem.API.Services
{
    public class TransactionServices : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAccountRepository AccountRepository;
        private readonly IUserRepository UserRepository;
        public TransactionServices(ITransactionRepository transactionRepository, IMapper mapper, IEmailService emailService, IUserRepository userRepository, IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentOutOfRangeException(nameof(transactionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            AccountRepository = accountRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

            var account = await AccountRepository.GetAccountByAccountNumberAsync(accountNumber);

            var user = await UserRepository.GetUserAsync(account.UserId);

            var emailBody = EmailTemplates.EmailBodyForTransactionDeposit(user.Fullname,transactionDto.Amount, transactionDto.TransactionRemarks, transactionDto.TransactionTime);

            // Prepare email
            var email = new Email
            {
                MailSubject = "Amount Deposited",
                MailBody = emailBody,
                ReceiverEmail = user.Email // Use the user's email address obtained from the UserDTO
            };

            // Send email
            await _emailService.SendEmailAsync(email);

            return await _transactionRepository.DepositTransactionAsync(transaction, accountNumber, userId);
        }

        public async Task<Transaction> WithdrawTransactionAsync(WithdrawTransactionDTO withdrawDto, long accountNumber, int atmIdAtmCardPin)
        {
            var transaction = _mapper.Map<Transaction>(withdrawDto);

            var account = await AccountRepository.GetAccountByAccountNumberAsync(accountNumber);

            var user = await UserRepository.GetUserAsync(account.UserId);

            var emailBody = EmailTemplates.EmailBodyForTransactionDeposit(user.Fullname, withdrawDto.Amount, withdrawDto.TransactionRemarks, withdrawDto.TransactionTime);

            // Prepare email
            var email = new Email
            {
                MailSubject = "Amount Withdrawn",
                MailBody = emailBody,
                ReceiverEmail = user.Email // Use the user's email address obtained from the UserDTO
            };

            // Send email
            await _emailService.SendEmailAsync(email);

            return await _transactionRepository.WithdrawTransactionAsync(transaction, accountNumber, atmIdAtmCardPin);
        }
    }
}
