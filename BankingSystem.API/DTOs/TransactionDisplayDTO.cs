namespace BankingSystem.API.DTOs
{
    public class TransactionDisplayDTO
    {
        public long AccountNumber { get; set; }
        public string UserName { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionTime { get; set; }
        public string? TransactionRemarks { get; set; }


        public TransactionDisplayDTO()
        { }
    }
}