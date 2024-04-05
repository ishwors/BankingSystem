using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs
{
    public class AccountUpdateDTO
    {
       // public long AccountNumber { get; set; }
        
        [Range(1000, 9999, ErrorMessage = "Number must be a four-digit integer.")]
        public int AtmCardPin { get; set; }
    }
}
