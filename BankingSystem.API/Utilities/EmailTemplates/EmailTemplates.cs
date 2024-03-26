using AutoMapper;
using BankingSystem.API.Data.Repository.IRepository;
using BankingSystem.API.Entities;
using BankingSystem.API.Services.IServices;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace BankingSystem.API.Utilities.EmailTemplates
{
    public class EmailTemplates
    {
        public static string EmailBodyForPinUpdate(int accountPin, string fullname)
        {
            return $"Dear {fullname},<br><br>Your bank account pin <b>{accountPin}</b> has been successfully changed.<br><br>" +
            "If you have any questions or need assistance, feel free to contact our support team.";
        }

        public static string EmailBodyForAddAccount(long accountNumber, long atmCardNumber, int atmCardPin)
        {
            return $"Dear user,<br><br>Your bank account has been successfully registered.<br><br>" +
                "Account number: " + accountNumber + "<br>" +
                "ATM number: " + atmCardNumber + "<br>" +
                "ATM PIN: " + atmCardPin + "<br><br>" +
                "Thank you for choosing our banking services. If you have any questions or need assistance, feel free to contact our support team.";
        }

        public static string EmailBodyForTransactionDeposit(string fullname, decimal depositAmount, string remarks, DateTime date)
        {
            return $"Dear {fullname},<br><br>Your bank account has been credited by <b>{depositAmount}</b> on {date}.<br><br>" +
            $"Remarks: {remarks}. <br><br>"+
            "~Small Bank";
        }

        public static string EmailBodyForTransactionWithdraw(string fullname, decimal withdrawAmount, string remarks, DateTime date)
        {
            return $"Dear {fullname},<br><br>Your bank account has been debited by <b>{withdrawAmount}</b> on {date}.<br><br>" +
           $"Remarks: {remarks}. <br><br>" +
           "~Small Bank";
        }
    }
}
