using BankingSystem.API.Entities;

namespace BankingSystem.API.DTOs
{
    public class WithdrawTransactionDTO
    {
        public TransactionType TransactionType = TransactionType.Withdraw;

        public decimal Amount { get; set; }

        public string ? TransactionRemarks { get; set; }

    }
}
