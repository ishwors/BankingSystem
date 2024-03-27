using BankingSystem.API.Entities;

namespace BankingSystem.API.DTOs
{
    public class DepositTransactionDTO
    {
        public TransactionType TransactionType = TransactionType.Deposit;

        public decimal Amount { get; set; }

        public string ? TransactionRemarks { get; set; }

    }
}
