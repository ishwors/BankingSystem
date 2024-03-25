﻿using BankingSystem.API.DTOs;
using BankingSystem.API.Entities;
using BankingSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{

    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountServices accountServices;
        private readonly UserService userServices;
        private readonly EmailService emailService;

        public AccountsController(AccountServices AccountServices, UserService UserService, EmailService _emailService)
        {
            accountServices = AccountServices ?? throw new ArgumentOutOfRangeException(nameof(AccountServices));
            userServices = UserService ?? throw new ArgumentOutOfRangeException(nameof(UserService));
            emailService = _emailService;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Accounts>>> GetAccounts()
        {
            if (await accountServices.GetAccountsAsync() == null)
            {
                var list = new List<Accounts>();
                return list;
            }

            return Ok(await accountServices.GetAccountsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Accounts>> GetAccountAsync(Guid id)
        {
            var account = await accountServices.GetAccountAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        /*[HttpPost]
        public async Task<ActionResult<Accounts>> AddAccounts(AccountDTO account, string email)
        {

            var user = await userServices.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound("User not found");
            }


            var userId = user.Id;

            var checkAccount = await accountServices.GetAccountByUserIdAsync(userId);
            if (checkAccount != null)
            {
                return StatusCode(400, "User already has an account.");
            }
            var accounts = await accountServices.AddAccounts(account);
            if (accounts == null)
            {
                return StatusCode(400, "Account already exists.");
            }

            var Email = new Email();
            Email.MailSubject = "Account Registered";
            Email.MailBody = "Your account has been made.";
            Email.ReceiverEmail = email;
            
           await emailService.SendEmailAsync(Email);
            return Ok(accounts);

        }*/

        [HttpDelete("{accountId}")]
        public ActionResult DeleteUser(Guid accountId)
        {
            accountServices.DeleteAccount(accountId);
            return NoContent();
        }

        [HttpPut("{accountId}")]
        public async Task<ActionResult<Accounts>> UpdateAccounts(Guid accountId, AccountDTO account)
        {
            var newAccount = await accountServices.UpdateAccountsAsync(accountId, account);
            if (newAccount == null)
            {
                return BadRequest("Update failed");
            }
            return Ok(newAccount);
        }

        /* [HttpPatch("{userId}")]
         public async Task<ActionResult<Accounts>> PatchAccountDetails(Guid accountId, JsonPatchDocument<AccountDTO> patchDocument)
         {
             var account = await accountServices.PatchAccountDetails(accountId, patchDocument);
             if (!Entitiestate.IsValid)
             {
                 return BadRequest(Entitiestate);
             }
             if (!TryValidateModel(account))
             {
                 return BadRequest(Entitiestate);
             }
             if (account == null)
             {
                 NotFound();
             }
             return Ok(account);
         }*/
        /*Route("api/send-email")]
        [HttpPost]
        public Task SendEmail()
        {
            var Email = new Email();
            Email.MailSubject = "Account Registered";
            Email.MailBody = "Your account has been made.";
            Email.SenderEmail = "aanisharai.aloi@gmail.com";
            return  emailService.SendEmailAsync(Email);
            

        }*/
    }
}
