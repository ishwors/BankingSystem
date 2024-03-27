using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs
{
    public class KycDocumentDTO
    {
        public Guid UserId { get; set; }

        [MaxLength(50)]
        public string? FatherName { get; set; }
        [MaxLength(50)]
        public string? MotherName { get; set; }

        [MaxLength(50)]
        public string? GrandFatherName { get; set; }

        public IFormFile? UserImageFile { get; set; }

        public IFormFile? CitizenshipImageFile { get; set; }

        [MaxLength(50)]
        public string? PermanentAddress { get; set; }

        public KycDocumentDTO()
        {
        }
    }
}
