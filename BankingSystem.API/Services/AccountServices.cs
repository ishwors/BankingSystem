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
    public class AccountServices : IAccountService
    {
        private readonly IAccountRepository AccountRepository;
        private readonly IUserRepository UserRepository;
        private readonly IEmailService _emailService;

        private readonly IMapper _mapper;
        public AccountServices(IAccountRepository accountRepository, IEmailService emailService, IMapper mapper, IUserRepository userRepository)
        {
            AccountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Accounts?> GetAccountAsync(Guid accountId)
        {
            return await AccountRepository.GetAccountAsync(accountId);
        }

        public async Task<IEnumerable<Accounts>> GetAccountsAsync(List<Guid> accountIds)
        {
            // Assuming some repository or data access method is used to fetch accounts
            // Replace this with your actual implementation to fetch accounts from the database or any other data source
            var accounts = await AccountRepository.GetAccountsByIdsAsync(accountIds);
            return accounts;
        }

        public async Task<IEnumerable<Accounts>> GetAccountsAsync()
        {
            return await AccountRepository.GetAccountsAsync();
        }
        public async Task<Accounts?> GetAccountByAccountNumberAsync(long accountNumber)
        {
            return await AccountRepository.GetAccountByAccountNumberAsync(accountNumber);
        }

        public async Task<Accounts?> GetAccountByUserIdAsync(Guid userId)
        {
            return await AccountRepository.GetAccountByUserIdAsync(userId);
        }

        public async Task<Accounts> AddAccounts(Accounts accounts, string useremail)
        {
            var finalAccount = _mapper.Map<Accounts>(accounts);
            var addedAccount = await AccountRepository.AddAccounts(finalAccount);
         
            var emailBody = EmailTemplates.EmailBodyForAddAccount(accounts.AccountNumber, accounts.AtmCardNum, accounts.AtmCardPin);

            // Prepare email
            var email = new Email
            {
                MailSubject = "Bank account registration Successful",
                MailBody = emailBody,
                ReceiverEmail = useremail // Use the user's email address obtained from the UserDTO
            };

            // Send email
            await _emailService.SendEmailAsync(email);

            return addedAccount;
        }

        public void DeleteAccount(Guid accountId)
        {
            AccountRepository.DeleteAccount(accountId);
        }

        public async Task<Accounts> UpdateAccountsAsync(Guid accountId, AccountUpdateDTO accounts)
        {
            var account = AccountRepository.GetAccountAsync(accountId).Result;
            account.AtmCardPin = accounts.AtmCardPin;
            account.ModifiedBy = account.UserId;
            account.ModifiedAt = DateTime.UtcNow;

            var user = await UserRepository.GetUserAsync(account.UserId);

            var emailBody = EmailTemplates.EmailBodyForPinUpdate(account.AtmCardPin, user.Fullname);
            // Prepare email
            var email = new Email
            {
                MailSubject = "Bank account pin updated",
                MailBody = emailBody,
                ReceiverEmail = user.Email // Use the user's email address obtained from the UserDTO
            };


            // Send email
            await _emailService.SendEmailAsync(email);

            return await AccountRepository.UpdateAccountsAsync(accountId, account);
        }
    }
}